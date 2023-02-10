using AutoMapper;
using CaaS.Api.Controllers.Common;
using CaaS.Api.Dtos;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace CaaS.Api.Controllers
{
    [ApiConventionType(typeof(WebApiConventions))]
    [Route("caas/shops/{shopId}/[controller]")]
    [ApiController]
    public class CartController : ControllerCaasBase
    {
        private readonly ICartManagementLogic logic;
        private readonly IMapper mapper;

        public CartController(ICartManagementLogic logic, IMapper mapper)
        {
            this.logic = logic ?? throw new ArgumentNullException(nameof(logic));
            this.mapper = mapper;
        }

        [HttpGet("{cartId}")]
        public async Task<CartDto?> GetCart([FromRoute] Guid shopId, [FromRoute] Guid cartId)
        {
            return (await logic.Get(shopId, cartId))?.ToDto();
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        public async Task<ActionResult<CartDto>> CreateCart([FromRoute] Guid shopId, [FromBody] CartDto cartDto)
        {
            if (cartDto.Entries.Any(entry => entry.Count < 1))
                return BadRequest(StatusInfo.AmountMustBeGreaterThanZero());
            var cart = cartDto.ToDomain();
            await logic.Add(shopId, cart);
            return CreatedAtAction(
                nameof(CreateCart),
                new { ShopId = shopId, CartId = cart.Id },
                cart.ToDto()
            );
        }

        [HttpDelete("{cartId}")]
        public async Task<ActionResult> DeleteCart([FromRoute] Guid shopID, [FromRoute] Guid cartId)
        {
            await logic.Delete(shopID, cartId);
            // no information for hackers
            return NoContent();
        }

        [HttpPut("{cartId}")]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult> UpdateCart(
            [FromRoute] Guid shopId, [FromRoute] Guid cartId, [FromBody] CartDto cartDto)
        {
            if (cartDto.Entries.Any(entry => entry.Count < 1))
                return BadRequest(StatusInfo.AmountMustBeGreaterThanZero());
            if (cartDto.Id == Guid.Empty)
                cartDto.Id = cartId;
            else if (cartId != cartDto.Id)
                return BadRequest(StatusInfo.InvalidCartId(cartId, cartDto.Id));
            await logic.Update(shopId, cartDto.ToDomain());
            return Ok();
        }

        [HttpGet("{cartId}/discounts")]
        public async Task<CartDiscountsDto> GetCartDiscounts([FromRoute] Guid shopId, [FromRoute] Guid cartId)
        {
            return mapper.Map<CartDiscountsDto>(await logic.GetDiscounts(shopId, cartId));
        }

        [HttpGet("{cartId}/sum")]
        public async Task<decimal> GetCartSum([FromRoute] Guid shopId, [FromRoute] Guid cartId)
        {
            return await logic.GetSum(shopId, cartId);
        }

        [HttpPost("{cartId}/checkout")]
        public async Task<ActionResult<Order?>> Checkout([FromRoute] Guid shopId, [FromRoute] Guid cartId, [FromBody] CustomerDto customerDto )
        {
            if (customerDto.Firstname.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("first name"));
            if (customerDto.Lastname.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("last name"));
            if (customerDto.Email.IsNullOrEmpty())
                return BadRequest(StatusInfo.StringEmpty("customer email"));
            return CreatedAtAction(
                nameof(Checkout), 
                await logic.Checkout(shopId, cartId, mapper.Map<Customer>(customerDto))
            );
        }
    }
}
