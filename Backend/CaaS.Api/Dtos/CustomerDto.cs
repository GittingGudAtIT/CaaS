using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class CustomerDto
    {
        [JsonProperty(Required = Required.Always)]
        public string Firstname { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string Lastname { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public string Email { get; set; } = string.Empty;
    }
}
