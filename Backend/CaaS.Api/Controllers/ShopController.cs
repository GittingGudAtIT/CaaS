using AutoMapper;
using CaaS.Api.Controllers.Common;
using CaaS.Api.Dtos;
using CaaS.Api.Dtos.RequestDtos;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Common;
using CaaS.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CaaS.Api.Controllers
{

    [ApiConventionType(typeof(WebApiConventions))]
    [Route("caas/[controller]")]
    [ApiController]
    public class ShopsController : ControllerCaasBase
    {
        private readonly IShopManagementLogic logic;
        private readonly IMapper mapper;

        public ShopsController(IShopManagementLogic logic, IMapper mapper)
        {
            this.logic = logic ?? throw new ArgumentNullException(nameof(logic));
            this.mapper = mapper;
        }


        [HttpGet("{shopId}")]
        public async Task<ActionResult<ShopDto>> GetShopById([FromRoute] Guid shopId)
        {
            var shop = await logic.Get(shopId);
            if (shop is null)
                return NotFound(StatusInfo.InvalidShopId(shopId));

            return mapper.Map<ShopDto>(shop);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ShopDto>> CreateShop([FromBody] ShopCreationDto shopDto)
        {
            if (shopDto.Name.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Shop name"));

            if (shopDto.AppKey.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Shop app key"));

            var shop = mapper.Map<Shop>(shopDto);
            await logic.Insert(shop);

            return CreatedAtAction(
                nameof(CreateShop),
                new { ShopId = shop.Id },
                mapper.Map<ShopDto>(shop)
            );
        }

        [HttpGet]
        public async Task<IEnumerable<ShopDto>> GetShops()
        {
            return mapper.Map<IEnumerable<ShopDto>>(await logic.GetAll());
        }

        [HttpPost("{shopId}/topsellers")]
        public async Task<IEnumerable<ProductAmountDto>> GetTopSellers(
            [FromRoute]Guid shopId, [FromBody] TopSellerRequest request)
        {
            return mapper.Map<IEnumerable<ProductAmountDto>>(
                await logic.GetTopSellers(shopId, request.Count, request.From, request.To)
            );
        }


        [HttpGet("{shopId}/orders/{orderId}")]
        public async Task<ActionResult<OrderDto>> GetOrder(
            [FromRoute] Guid shopId, [FromRoute] Guid orderId)
        {
            var order = await logic.GetOrder(shopId, orderId);
            if (order is null)
                return NotFound(StatusInfo.InvalidOrderId(orderId));

            return mapper.Map<OrderDto>(order);
        }

        [HttpPut("{shopId}/administration")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateShop(
            [FromRoute] Guid shopId, [FromBody] ShopUpdateRequestDto request)
        {

            if (request.Shop.Name.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Shop name"));

            if (request.Shop.AppKey.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Shop app key"));

            var shop = mapper.Map<Shop>(request.Shop);
            shop.Id = shopId;
            return ResultFromRequestResult(await logic.Update(shop, request.AppKey));
        }

        [HttpDelete("{shopId}")]
        public async Task<ActionResult> DeleteShop([FromRoute] Guid shopId, [FromBody] AppKey appKey)
        {
            await logic.Delete(shopId, appKey.Key);
            //could handle here, but don't want to give attackers information
            return NoContent();
        }

        [HttpPost("{shopId}/administration/statistics/sales")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<decimal?>> GetSales(
            [FromRoute] Guid shopId, [FromBody] TimeRangeRequestDto request)
        {
            if (request.To <= request.From)
                return BadRequest();
            var result = await logic.EvaluateSales(shopId, request.AppKey, request.From, request.To);
            return ResultFromRequestResult(result.RequestResult, result.Value);
        }

        [HttpPost("{shopId}/administration/statistics/cartsalesdistributed")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<WeekDayDistribution<decimal>?>> GetCartSalesDistribution(
            [FromRoute] Guid shopId, [FromBody] TimeRangeRequestDto request)
        {
            var result = await logic.EvaluateCartSales(shopId, request.AppKey, request.From, request.To);
            return ResultFromRequestResult(result.RequestResult, result.Value);

        }

        [HttpPost("{shopId}/administration/statistics/cartcountsdistributed")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<WeekDayDistribution<decimal>?>> GetCartCountsDistribution(
            [FromRoute] Guid shopId, [FromBody] TimeRangeRequestDto request)
        {
            var result = await logic.EvaluateCartCounts(shopId, request.AppKey, request.From, request.To);
            return ResultFromRequestResult(result.RequestResult, result.Value);

        }

        [HttpPost("{shopId}/administration/orders")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders(
            [FromRoute] Guid shopId, [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string? pattern, [FromBody] AppKey appKey)
        {
            var result = string.IsNullOrEmpty(pattern) ?
                await logic.GetAllOrders(shopId, appKey.Key, from, to) :
                await logic.SeachOrders(shopId, appKey.Key, from, to, pattern);
            var value = mapper.Map<IEnumerable<OrderDto>>(result.Value ?? Enumerable.Empty<Order>());
            return ResultFromRequestResult(result.RequestResult, value);
        }

        [HttpPost("{shopId}/administration/login")]
        public async Task<bool> CheckIfAppKeyFits([FromRoute]Guid shopId, [FromBody]AppKey appKey)
        {
            var shop = await logic.Get(shopId);
            if (shop != null && shop.AppKey == appKey.Key)
                return true;

            return false;
        }
    }
}
