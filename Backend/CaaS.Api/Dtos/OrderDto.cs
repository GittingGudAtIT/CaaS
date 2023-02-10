using CaaS.Core.Domain;
using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class OrderDto
    {
        [JsonProperty(Required = Required.DisallowNull)]
        public Guid Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime DateTime { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public decimal OffSum { get; set; }

        [JsonProperty(Required = Required.Always)]
        public CustomerDto Customer { get; set; } = null!;

        [JsonProperty(Required = Required.Always)]
        public IEnumerable<OrderEntryDto> Entries { get; set; } = Array.Empty<OrderEntryDto>();

        [JsonProperty(Required = Required.Always)]
        public decimal Total { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DownloadLink { get; set; } = string.Empty;
    }
}
