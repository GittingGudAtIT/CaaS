using AutoMapper;
using CaaS.Api.Controllers.Common;
using CaaS.Api.Dtos;
using CaaS.Api.Dtos.RequestDtos;
using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.DBLayer.Domain;
using CaaS.Core.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CaaS.Api.Controllers
{
    [ApiConventionType(typeof(WebApiConventions))]
    [Route("caas/shops/{shopId}/[controller]")]
    [ApiController]
    public class DiscountsController : ControllerCaasBase
    {
        private readonly IDiscountManagementLogic logic;
        private readonly IMapper mapper;

        public DiscountsController(IDiscountManagementLogic logic, IMapper mapper)
        {
            this.logic = logic ?? throw new ArgumentNullException(nameof(logic));
            this.mapper = mapper;
        }

        [HttpGet("{discountId}")]
        public async Task<DiscountDto?> GetById([FromRoute] Guid shopId, [FromRoute] Guid discountId)
        {
            return mapper.Map<DiscountDto>(await logic.Get(shopId, discountId));
        }

        [HttpGet]
        public async Task<IEnumerable<DiscountDto>> Search(
            [FromRoute] Guid shopId, [FromQuery] string? pattern, 
            [FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            DateTime start = from?? DateTime.MinValue;
            DateTime end = to?? DateTime.MaxValue;

            if (start >= end)
                return Enumerable.Empty<DiscountDto>();

            return mapper.Map<IEnumerable<DiscountDto>>(await logic.Search(shopId, pattern, start, end));
        }

        [HttpGet("allactive")]
        public async Task<IEnumerable<DiscountDto>> GetAllActive([FromRoute] Guid shopId)
        {
            return mapper.Map<IEnumerable<DiscountDto>>(await logic.GetAllActive(shopId));
        }

        [HttpPut("{discountId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateDiscount(
            [FromRoute] Guid shopId, [FromBody] DiscountUpdateRequest request)
        {
            if (request.Discount.FreeProducts.Any(pa => pa.Count < 1))
                return BadRequest(StatusInfo.AmountMustBeGreaterThanZero());

            return ResultFromRequestResult(await logic.Update(
                shopId, request.AppKey, mapper.Map<Discount>(request.Discount)
            ));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<DiscountDto>> CreateDiscount(
            [FromRoute] Guid shopId, [FromBody] DiscountUpdateRequest request)
        {
            var discount = mapper.Map<Discount>(request.Discount);

            if (!discount.IsValid())
                return BadRequest(StatusInfo.InvalidDiscountConfiguration());

            var reqResult = await logic.Insert(
                shopId, request.AppKey, discount
            );

            if(reqResult == RequestResult.Success)
            {
                return CreatedAtAction(
                    nameof(CreateDiscount),
                    new { DiscountId = discount.Id },
                    mapper.Map<DiscountDto>(discount)
                );
            }
            return ResultFromRequestResult(reqResult);
        }

        [HttpDelete("{discountId}")]
        public async Task<ActionResult> DeleteDiscount(
            [FromRoute] Guid shopId, [FromRoute] Guid discountId, [FromBody] AppKey appKey)
        {
            await logic.Delete(shopId, appKey.Key, discountId);
            //could handle here, but don't want to give attackers information
            return NoContent();
        }
    }
}
