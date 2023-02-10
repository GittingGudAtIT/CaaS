

using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class ProductUpdateRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public decimal Price { get; set; }
    }
}
