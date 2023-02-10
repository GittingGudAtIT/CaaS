using CaaS.Core.Domain;
using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class ProductAmountDto
    {
        [JsonProperty(Required = Required.Always)]
        public ProductDto Product { get; set; } = null!;

        [JsonProperty(Required = Required.Always)]
        public int Count { get; set; }
    }
}
