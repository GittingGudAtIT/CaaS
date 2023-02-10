using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class DiscountLookupProfile : Profile
    {
        public DiscountLookupProfile()
        {
            CreateMap<DiscountLookup, DiscountLookupDto>();
        }
    }
}
