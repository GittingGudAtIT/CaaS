using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Interface
{
    public interface IProductDao
    {
        Task<IEnumerable<Product>> FindAllAsync(Guid shopId);
        Task<Product?> FindByIdAsync(Guid shopId, Guid productId);
        Task<IEnumerable<Product>> FindByIdAsync(Guid shopId, IEnumerable<Guid> productIds);
        Task InsertAsync(Guid shopId, Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Guid productId);
        Task<IEnumerable<Product>> SearchAsync(Guid shopId, string pattern);
        Task<Shop?> FindShopFromProductAsync(Guid productId);
    }
}
