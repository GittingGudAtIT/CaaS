using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class ProductDto
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public Guid Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(Required = Required.DisallowNull)]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public decimal Price { get; set; }

        public int ImageNr { get; set; }
        
    }
}
