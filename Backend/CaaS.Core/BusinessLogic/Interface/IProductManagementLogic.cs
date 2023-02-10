using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Interface
{
    public interface IProductManagementLogic
    {
        Task<Product?> Get(Guid shopId, Guid id);
        Task<IEnumerable<Product>> Get(Guid shopId, IEnumerable<Guid> ids);
        Task<IEnumerable<Product>> GetAll(Guid shopId);
        Task<IEnumerable<Product>> Search(Guid shopId, string pattern);
        Task<IEnumerable<DiscountLookup>> GetDiscountsForProducts(Guid shopId, IEnumerable<Guid> productIds);
        Task<IEnumerable<Discount>> GetDiscountsForProduct(Guid shopId, Guid id);

        // admin only

        Task<MayDenied<Product?>> Get(Guid shopId, Guid id, string appKey);
        Task<RequestResult> Update(Guid shopId, string appKey, Product product);
        Task<RequestResult> Delete(Guid shopId, string appKey, Guid id);
        Task<RequestResult> Insert(Guid shopId, string appKey, Product product);
    }
}
