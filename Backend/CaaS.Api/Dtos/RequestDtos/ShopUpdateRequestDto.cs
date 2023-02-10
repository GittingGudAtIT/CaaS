using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class ShopUpdateRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;


        [JsonProperty(Required = Required.Always)]
        public ShopCreationDto Shop { get; set; } = null!;
    }
}
