using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Interface
{
    public interface ICartDao
    {
        Task<Cart?> FindByIdAsync(Guid shopId, Guid cartId);
        Task InsertAsync(Guid shopId, Cart cart);
        Task DeleteAsync(Guid shopId, Guid cartId);
        Task UpdateAsync(Guid shopId, Cart cart);
        Task<Shop?> FindShopFromCartAsync(Guid cartId);
    }
}
