using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class ProductProfile : Profile
    {
        public ProductProfile()
        {
            CreateMap<Product, ProductDto>();
            CreateMap<ProductDto, Product>();
            CreateMap<Product, ProductAdminDto>();
            CreateMap<ProductAdminDto, Product>();
            
        }
    }
}
