using CaaS.Core.DBLayer.Domain;
using CaaS.Core.Domain;
using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class DiscountDto
    {
        public Guid? Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public OffType OffType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public decimal OffValue { get; set; }

        [JsonProperty(Required = Required.DisallowNull)]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(Required = Required.DisallowNull)]
        public string Tag { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public MinType MinType { get; set; }

        [JsonProperty(Required = Required.Always)]
        public decimal MinValue { get; set; }

        [JsonProperty(Required = Required.Always)]
        public bool Is4AllProducts { get; set; }


        public IEnumerable<ProductAmountDto> FreeProducts { get; set; } = Enumerable.Empty<ProductAmountDto>();


        public IEnumerable<Guid> Products { get; set; } = Enumerable.Empty<Guid>();

        [JsonProperty(Required = Required.Always)]
        public DateTime ValidFrom { get; set; }

        [JsonProperty(Required = Required.Always)]
        public DateTime ValidTo { get; set; }
    }
}