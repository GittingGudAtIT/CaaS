using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Interface
{
    public interface ICartManagementLogic
    {
        Task<Cart?> Get(Guid shopId, Guid id);
        Task Add(Guid shopId, Cart cart);
        Task Delete(Guid shopId, Guid id);
        Task Update(Guid shopId, Cart cart);
        Task<CartDiscounts> GetDiscounts(Guid shopId, Guid id);
        Task<decimal> GetSum(Guid shopId, Guid id);
        Task<Order?> Checkout(Guid shopId, Guid id, Customer customer);
    }
}
