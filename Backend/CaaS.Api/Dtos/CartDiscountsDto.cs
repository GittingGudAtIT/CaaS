using CaaS.Core.Domain;
using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class CartDiscountsDto
    {
        [JsonProperty(Required = Required.Default)]
        public IEnumerable<DiscountLookupDto>? ProductDiscounts { get; set; }

        [JsonProperty(Required = Required.Default)]
        public IEnumerable<DiscountWOProductsDto>? ValueDiscounts { get; set; }
    }
}
