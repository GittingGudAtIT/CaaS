using CaaS.Core.Domain;
using CaaS.Core.Dal.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Interface
{
    public interface IShopDao
    {
        Task<IEnumerable<Shop>> FindAllAsync();
        Task<Shop?> FindByIdAsync(Guid id);
        Task UpdateAsync(Shop shop);
        Task InsertAsync(Shop shop);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<ProductAmount>> EvaluateTopsellersAsync(Guid id, int count, DateTime start, DateTime end);
        Task<decimal> EvaluateSalesAsync(Guid id, DateTime start, DateTime end);
        Task<WeekDayDistribution<decimal>> EvaluateCartSalesDistributed(Guid id, DateTime start, DateTime end);
        Task<WeekDayDistribution<decimal>> EvaluateCartProductCountDistributed(Guid id, DateTime start, DateTime end);
    }
}
