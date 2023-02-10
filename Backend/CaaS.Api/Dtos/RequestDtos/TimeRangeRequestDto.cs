using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class TimeRangeRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public string AppKey { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public DateTime From { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime To { get; set; }
    }
}
