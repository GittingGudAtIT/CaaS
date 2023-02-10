using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Interface
{
    public interface IDiscountManagementLogic
    {
        Task<Discount?> Get(Guid shopId, Guid id);
        Task<IEnumerable<Discount>> GetAll(Guid shopId);
        Task<IEnumerable<Discount>> GetAllActive(Guid shopId);
        Task<IEnumerable<Discount>> Search(Guid shopId, string? pattern, DateTime from, DateTime to);

        // admin

        Task<RequestResult> Update(Guid shopId, string appKey, Discount discount);
        Task<RequestResult> Insert(Guid shopId, string appKey, Discount discount);
        Task<RequestResult> Delete(Guid shopId, string appKey, Guid id);
    }
}
