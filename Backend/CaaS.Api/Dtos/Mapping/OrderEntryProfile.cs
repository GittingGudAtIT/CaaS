using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class OrderEntryProfile : Profile
    {
        public OrderEntryProfile()
        {
            CreateMap<OrderEntry, OrderEntryDto>();
            CreateMap<OrderEntryDto, OrderEntry>();
        }
    }
}
