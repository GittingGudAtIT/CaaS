using AutoMapper;
using CaaS.Core.Domain;

namespace CaaS.Api.Dtos.Mapping
{
    public class CartDiscountsProfile : Profile
    {
        public CartDiscountsProfile()
        {
            CreateMap<CartDiscounts, CartDiscountsDto>();
        }
    }
}
