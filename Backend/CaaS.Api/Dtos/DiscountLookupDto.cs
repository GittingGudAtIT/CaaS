using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class DiscountLookupDto
    {
        [JsonProperty(Required = Required.Always)]
        public DiscountWOProductsDto Discount { get; set; } = null!;

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<Guid> ProductIds { get; set; } = null!;
    }
}
