using CaaS.Core.Domain;
using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class OrderEntryDto
    {
        [JsonProperty(Required = Required.Always)]
        public int RowNumber { get; set; }

        [JsonProperty(Required = Required.Always)]
        public int Count { get; set; }

        [JsonProperty(Required = Required.Always)]
        public OrderProductDto Product { get; set; } = null!;
    }
}