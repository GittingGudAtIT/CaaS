using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class ShopDto
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public Guid Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;
    }
}
