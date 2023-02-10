using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class ProductAmountProfile : Profile
    {
        public ProductAmountProfile()
        {
            CreateMap<ProductAmount, ProductAmountDto>();
            CreateMap<ProductAmountDto, ProductAmount>();
        }
    }
}
