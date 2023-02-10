using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class ProductCreateRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public ProductAdminDto Product { get; set; } = null!;


        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;
    }
}
