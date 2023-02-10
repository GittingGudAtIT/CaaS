using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class OrderProductDto
    {
        [JsonProperty(Required = Required.Always)]
        public Guid OriginalId { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public decimal Price { get; set; }
    }
}