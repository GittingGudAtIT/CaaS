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
    public class DiscountManagementLogic : IDiscountManagementLogic
    {
        private readonly IDiscountDao discountDao;
        private readonly IShopDao shopDao;

        public DiscountManagementLogic()
        {
            var connectionFactory = Connection.GetFactory();
            discountDao = new AdoDiscountDao(connectionFactory);
            shopDao = new AdoShopDao(connectionFactory);
        }

        public DiscountManagementLogic(IDiscountDao discountDao, IShopDao shopDao)
        {
            this.discountDao = discountDao;
            this.shopDao = shopDao;
        }

        private async Task<RequestResult> CheckAdminRequest(Guid shopId, string appKey, Guid id)
        {
            var shop = await discountDao.FindShopFromDiscountAsync(id);
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
                await discountDao.DeleteAsync(id);
            return result;
        }

        public async Task<Discount?> Get(Guid shopId, Guid id)
        {
            return await discountDao.FindByIdAsync(shopId, id);
        }

        public async Task<IEnumerable<Discount>> GetAll(Guid shopId)
        {
            return await discountDao.FindAllAsync(shopId);
        }

        public async Task<IEnumerable<Discount>> Search(Guid shopId, string? pattern, DateTime from, DateTime to)
        {
            if (pattern is null || pattern == string.Empty)
                return await discountDao.SearchAsync(shopId, from, to);
            return await discountDao.SearchAsync(shopId, pattern, from, to);
        }

        public async Task<IEnumerable<Discount>> GetAllActive(Guid shopId)
        {
            return await discountDao.FindAllActiveAsync(shopId);
        }

        public async Task<RequestResult> Insert(Guid shopId, string appKey, Discount discount)
        {
            var result = await ShopManagementLogic.CheckAdminRequest(shopDao, shopId, appKey);
            if (result == RequestResult.Success)
                await discountDao.InsertAsync(shopId, discount);
            return result;
        }


        public async Task<RequestResult> Update(Guid shopId, string appKey, Discount discount)
        {
            var result = await CheckAdminRequest(shopId, appKey, discount.Id);
            if (result == RequestResult.Success)
                await discountDao.UpdateAsync(discount);
            return result;
        }
    }
}
