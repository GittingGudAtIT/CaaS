using AutoMapper;
using CaaS.Api.Controllers.Common;
using CaaS.Api.Dtos;
using CaaS.Api.Dtos.RequestDtos;
using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CaaS.Api.Controllers
{
    [ApiConventionType(typeof(WebApiConventions))]
    [Route("caas/shops/{shopId}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerCaasBase
    {
        private readonly IProductManagementLogic logic;
        private readonly IMapper mapper;

        public ProductsController(IProductManagementLogic logic, IMapper mapper)
        {
            this.logic = logic ?? throw new ArgumentNullException(nameof(logic));
            this.mapper = mapper;
        }

        [HttpGet("{productId}")]
        public async Task<ProductDto?> GetById([FromRoute] Guid shopId, [FromRoute] Guid productId)
        {
            return mapper.Map<ProductDto?>(await logic.Get(shopId, productId));
        }


        [HttpPost("range")]
        public async Task<IEnumerable<ProductDto>> GetByIdRange([FromRoute] Guid shopId, [FromBody] IEnumerable<Guid> productIds)
        {
            return mapper.Map<IEnumerable<ProductDto>>(await logic.Get(shopId, productIds));
        }


        [HttpGet]
        public async Task<IEnumerable<ProductDto>> SearchProducts([FromRoute] Guid shopId, [FromQuery] string? pattern)
        {
            if (pattern is null || pattern == string.Empty)
                return mapper.Map<IEnumerable<ProductDto>>(await logic.GetAll(shopId));
            return mapper.Map<IEnumerable<ProductDto>>(await logic.Search(shopId, pattern));
        }

        [HttpPost("{productId}/admin")]
        public async Task<ActionResult<ProductAdminDto>> GetByIdAdmin([FromRoute] Guid shopId, [FromRoute] Guid productId, [FromBody] AppKey appKey)
        {
            var res = await logic.Get(shopId, productId, appKey.Key);
            if (res.RequestResult == RequestResult.Success)
                return mapper.Map<ProductAdminDto>(res.Value);
            return ResultFromRequestResult(res.RequestResult);
        }


        [HttpPost("discountsforrange")]
        public async Task<IEnumerable<DiscountLookupDto>> GetDiscountsForProducts(
            [FromRoute] Guid shopId, [FromBody] IEnumerable<Guid>? productIds)
        {
            if (productIds is null || !productIds.Any())
                return Enumerable.Empty<DiscountLookupDto>();
            return mapper.Map<IEnumerable<DiscountLookupDto>>(await logic.GetDiscountsForProducts(shopId, productIds));
        }

        [HttpGet("{productId}/discounts")]
        public async Task<IEnumerable<DiscountWOProductsDto>> GetDiscountsForProduct(
            [FromRoute] Guid shopId, [FromRoute] Guid productId)
        {
            return mapper.Map<IEnumerable<DiscountWOProductsDto>>(await logic.GetDiscountsForProduct(shopId, productId));
        }

        [HttpPut("{productId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateProduct(
            [FromRoute] Guid shopId, [FromRoute] Guid productId, [FromBody] ProductUpdateRequestDto request)
        {
            if (request.Price < 0)
                return BadRequest(StatusInfo.PriceCantBeSmallerThanZero());
            var prod = new Product(productId, "", request.Price, "");
            return ResultFromRequestResult(await logic.Update(shopId, request.AppKey, prod));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ProductAdminDto>> CreateProduct(
            [FromRoute] Guid shopId, [FromBody] ProductCreateRequestDto request)
        {
            if (request.Product.Price < 0)
                return BadRequest(StatusInfo.PriceCantBeSmallerThanZero());

            if (request.Product.Name.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Product Name"));

            if (request.Product.DownloadLink.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("Product Downloadlink"));

            var prod = mapper.Map<Product>(request.Product);
            var reqResult = await logic.Insert(shopId, request.AppKey, prod);

            if(reqResult == RequestResult.Success)
            {
                var returnProd = request.Product;
                returnProd.Id = prod.Id;
                return CreatedAtAction(
                    nameof(CreateProduct),
                    new { ShopId = shopId, ProductId = prod.Id },
                    returnProd
                );
            }
            return ResultFromRequestResult(reqResult);
        }

        [HttpDelete("{productId}")]
        public async Task<ActionResult> DeleteProduct(
            [FromRoute] Guid shopId, [FromRoute] Guid productId, [FromBody] AppKey appKey)
        {
            await logic.Delete(shopId, appKey.Key, productId);
            //could handle here, but don't want to give attackers information
            return NoContent();
        }
    }
}
