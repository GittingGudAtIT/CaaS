using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class ShopCreationDto
    {
        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;
    }
}
