using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class AppKey
    {
        [JsonProperty(Required = Required.Always)]
        public string Key { get; set; } = null!;
    }
}
