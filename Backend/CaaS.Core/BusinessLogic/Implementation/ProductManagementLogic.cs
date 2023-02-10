using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Implementation
{
    public class ProductManagementLogic : IProductManagementLogic
    {
        private readonly IProductDao productDao;
        private readonly IDiscountDao discountDao;
        private readonly IShopDao shopDao;

        public ProductManagementLogic()
        {
            var connectionFactory = Connection.GetFactory();
            productDao = new AdoProductDao(connectionFactory);
            discountDao = new AdoDiscountDao(connectionFactory);
            shopDao = new AdoShopDao(connectionFactory);
        }

        public ProductManagementLogic(IProductDao productDao, IDiscountDao discountDao, IShopDao shopDao)
        {
            this.productDao = productDao;
            this.discountDao = discountDao;
            this.shopDao = shopDao;
        }

        private async Task<RequestResult> CheckAdminRequest(Guid shopId, string appKey, Guid id)
        {
            var shop = await productDao.FindShopFromProductAsync(id);
            if (shop is null || shop.Id != shopId)
                return RequestResult.Failure;
            else if (shop.AppKey != appKey)
                return RequestResult.NoPermission;
            return RequestResult.Success;
        }

        public async Task<RequestResult> Delete(Guid shopId, string appKey, Guid id)
        {
            var result = await CheckAdminRequest(shopId, appKey, id);
            if (result == RequestResult.Success)
                await productDao.DeleteAsync(id);
            return result;
        }

        public async Task<Product?> Get(Guid shopId, Guid id)
        {
            return await productDao.FindByIdAsync(shopId, id);
        }

        public async Task<MayDenied<Product?>> Get(Guid shopId, Guid id, string appKey)
        {
            var res = new MayDenied<Product?>(await CheckAdminRequest(shopId, appKey, id));
            if (res.RequestResult == RequestResult.Success)
                res.Value = await Get(shopId, id);
            return res;
        }

        public async Task<IEnumerable<Product>> Get(Guid shopId, IEnumerable<Guid> ids)
        {
            return await productDao.FindByIdAsync(shopId, ids);
        }

        public Task<IEnumerable<Product>> GetAll(Guid shopId)
        {
            return productDao.FindAllAsync(shopId);
        }

        public async Task<IEnumerable<Discount>> GetDiscountsForProduct(Guid shopId, Guid id)
        {
            return await discountDao.FindForProductAsync(shopId, id);
        }

        public async Task<IEnumerable<DiscountLookup>> GetDiscountsForProducts(Guid shopId, IEnumerable<Guid> productIds)
        {
            return await discountDao.FindForProductsAsync(shopId, productIds);
        }

        public async Task<RequestResult> Insert(Guid shopId, string appKey, Product product)
        {
            var result = await ShopManagementLogic.CheckAdminRequest(shopDao, shopId, appKey);
            if (result == RequestResult.Success)
                await productDao.InsertAsync(shopId, product);
            return result;
        }

        public async Task<IEnumerable<Product>> Search(Guid shopId, string pattern)
        {
            return await productDao.SearchAsync(shopId, pattern);
        }

        public async Task<RequestResult> Update(Guid shopId, string appKey, Product product)
        {
            var result = await CheckAdminRequest(shopId, appKey, product.Id);
            if (result == RequestResult.Success)
                await productDao.UpdateAsync(product);
            return result;
        }
    }
}
