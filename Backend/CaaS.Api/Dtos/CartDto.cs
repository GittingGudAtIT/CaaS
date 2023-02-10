using AutoMapper;
using CaaS.Core.Domain;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

namespace CaaS.Api.Dtos
{
    public class CartDto
    {
        [JsonProperty(Required = Required.AllowNull)]
        public Guid? Id { get; set; }


        [JsonProperty(Required = Required.Always)]
        public IEnumerable<ProductAmountDto> Entries { get; set; } = null!;

        public Cart ToDomain()
        {
            return new(Id?? Guid.Empty, Entries.Select(x => new ProductAmount(
                new Product(
                    x.Product.Id,
                    x.Product.Name,
                    x.Product.Price,
                    x.Product.Description,
                    x.Product.ImageNr
                ), x.Count
            )));
        }
    }

    public static class CartExtensions
    {
        public static CartDto ToDto(this Cart cart)
        {
            return new()
            {
                Id = cart.Id,
                Entries = cart.Select(
                    x => new ProductAmountDto()
                    {
                        Count = x.Count,
                        Product = new ProductDto()
                        {
                            Name = x.Product.Name,
                            Id = x.Product.Id,
                            Description = x.Product.Description,
                            Price = x.Product.Price,
                            ImageNr = x.Product.ImageNr
                        }
                    }
                )
            };
        }
    }
}
