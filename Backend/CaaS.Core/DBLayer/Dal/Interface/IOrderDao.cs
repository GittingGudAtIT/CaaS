using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Interface
{
    public interface IOrderDao
    {
        Task<IEnumerable<Order>> FindAllForShopAsync(Guid shopId, DateTime from, DateTime to);
        Task<Order?> FindByIdAsync(Guid shopId, Guid id);
        Task InsertAsync(Guid shopId, Order order);
        Task<IEnumerable<Order>> SearchOrders(Guid shopId, DateTime from, DateTime to, string searchTerm);
    }
}
