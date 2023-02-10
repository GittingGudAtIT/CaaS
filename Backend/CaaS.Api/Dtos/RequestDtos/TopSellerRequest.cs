using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class TopSellerRequest
    {
        [JsonProperty(Required = Required.Always)]
        public DateTime From { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime To { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Count { get; set; }
    }
}
