using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class DiscountUpdateRequest
    {
        [JsonProperty(Required = Required.Always)]
        public DiscountDto Discount { get; set; } = null!;


        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;
    }
}
