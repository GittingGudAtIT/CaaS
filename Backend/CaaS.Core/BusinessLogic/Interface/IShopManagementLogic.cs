using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.Dal.Common;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Interface
{
    
    public interface IShopManagementLogic
    {
        Task<Shop?> Get(Guid id);
        Task Insert(Shop shop);
        Task<IEnumerable<Shop>> GetAll();
        Task<IEnumerable<ProductAmount>> GetTopSellers(Guid shopId, int count, DateTime start, DateTime end);
        Task<Order?> GetOrder(Guid shopId, Guid id);


        // admin only

        Task<RequestResult> Update(Shop shop, string appKey);
        Task<RequestResult> Delete(Guid id, string appkey);
        Task<MayDenied<decimal?>> EvaluateSales(Guid shopId, string appKey, DateTime start, DateTime end);
        Task<MayDenied<WeekDayDistribution<decimal>>> EvaluateCartSales(Guid shopId, string appKey, DateTime start, DateTime end);
        Task<MayDenied<WeekDayDistribution<decimal>>> EvaluateCartCounts(Guid shopId, string appKey, DateTime start, DateTime end);
        Task<MayDenied<IEnumerable<Order>>> GetAllOrders(Guid shopId, string appKey, DateTime from, DateTime to);
        Task<MayDenied<IEnumerable<Order>>> SeachOrders(Guid shopId, string appKey, DateTime from, DateTime to, string searchTerm);

    }
}
