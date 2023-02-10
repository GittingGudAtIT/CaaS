using Newtonsoft.Json;

namespace CaaS.Api.Dtos.RequestDtos
{
    public class TimeRangeLimitedCountRequestDto : TimeRangeRequestDto
    {
        [JsonProperty(Required = Required.Always)]
        public int Count { get; set; }
    }
}
