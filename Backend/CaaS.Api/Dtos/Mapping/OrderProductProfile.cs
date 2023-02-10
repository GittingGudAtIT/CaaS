using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class OrderProductProfilel : Profile
    {
        public OrderProductProfilel()
        {
            CreateMap<OrderProduct, OrderProductDto>();
            CreateMap<OrderProductDto, OrderProduct>();
        }
    }
}
