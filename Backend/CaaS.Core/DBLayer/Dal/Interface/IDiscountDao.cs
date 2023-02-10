using CaaS.Core.Dal.Common;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Dal.Interface
{
    public interface IDiscountDao
    {
        Task<Discount?> FindByIdAsync(Guid shopId, Guid id);
        Task<IEnumerable<Discount>> FindAllAsync(Guid shopId);
        Task<IEnumerable<Discount>> FindAllActiveAsync(Guid shopId);
        Task<IEnumerable<Discount>> SearchAsync(Guid shopId, DateTime from, DateTime to);
        Task<IEnumerable<Discount>> SearchAsync(Guid shopId, string pattern, DateTime from, DateTime to);
        Task InsertAsync(Guid shopid, Discount discount);
        Task UpdateAsync(Discount discount);
        Task DeleteAsync(Guid id);
        Task<IEnumerable<Discount>> FindValueDiscountsForCartAsync(Guid shopId, Guid cartId);
        Task<IEnumerable<DiscountLookup>> FindProductDiscountsForCart(Guid shopId, Guid cartId);
        Task<IEnumerable<DiscountLookup>> FindForProductsAsync(Guid shopId, IEnumerable<Guid> productIds);
        Task<IEnumerable<Discount>> FindForProductAsync(Guid shopId, Guid productId);
        Task<Shop?> FindShopFromDiscountAsync(Guid discountId);
    }
}
