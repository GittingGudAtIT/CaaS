using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class DiscountProfile : Profile
    {
        public DiscountProfile()
        {
            CreateMap<Discount, DiscountDto>();
            CreateMap<DiscountDto, Discount>();
            CreateMap<Discount, DiscountWOProductsDto>();
        }
    }
}
