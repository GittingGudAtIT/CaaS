using Newtonsoft.Json;

namespace CaaS.Api.Dtos
{
    public class ProductAdminDto
    {
        public Guid? Id { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string Name { get; set; } = string.Empty;

        [JsonProperty(Required = Required.DisallowNull)]
        public string Description { get; set; } = string.Empty;

        [JsonProperty(Required = Required.Always)]
        public decimal Price { get; set; }

        [JsonProperty(Required = Required.Always)]
        public string DownloadLink { get; set; } = string.Empty;

        public int ImageNr { get; set; }
    }
}
