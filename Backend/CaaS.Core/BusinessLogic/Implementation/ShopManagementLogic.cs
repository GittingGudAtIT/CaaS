using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Implementation
{
    public class ShopManagementLogic : IShopManagementLogic
    {
        private readonly IShopDao shopDao;
        private readonly IOrderDao orderDao;

        public ShopManagementLogic()
        {
            var connectionFactory = Connection.GetFactory();
            shopDao = new AdoShopDao(connectionFactory);
            orderDao = new AdoOrderDao(connectionFactory);
        }

        public ShopManagementLogic(IShopDao shopDao, IOrderDao orderDao)
        {
            this.shopDao = shopDao;
            this.orderDao = orderDao;
        }

        public async static Task<RequestResult> CheckAdminRequest(IShopDao shopDao, Guid shopId, string appKey)
        {
            var shop = await shopDao.FindByIdAsync(shopId);
            if (shop is null)
                return RequestResult.Failure;
            else if (shop.AppKey != appKey)
                return RequestResult.NoPermission;
            return RequestResult.Success;
        }

        public async Task<RequestResult> Delete(Guid id, string appKey)
        {
            var result = await CheckAdminRequest(shopDao, id, appKey);
            if (result == RequestResult.Success)
                await shopDao.DeleteAsync(id);
            return result;
        }

        public async Task<MayDenied<WeekDayDistribution<decimal>>> EvaluateCartCounts(Guid shopId, string appKey, DateTime start, DateTime end)
        {
            var result = new MayDenied<WeekDayDistribution<decimal>>(await CheckAdminRequest(shopDao, shopId, appKey));
            if (result.RequestResult == RequestResult.Success)
                result.Value = await shopDao.EvaluateCartProductCountDistributed(shopId, start, end);
            return result;
        }

        public async Task<MayDenied<WeekDayDistribution<decimal>>> EvaluateCartSales(Guid shopId, string appKey, DateTime start, DateTime end)
        {
            var result = new MayDenied<WeekDayDistribution<decimal>>(await CheckAdminRequest(shopDao, shopId, appKey));
            if (result.RequestResult == RequestResult.Success)
                result.Value = await shopDao.EvaluateCartSalesDistributed(shopId, start, end);
            return result;
        }

        public async Task<MayDenied<decimal?>> EvaluateSales(Guid shopId, string appKey, DateTime start, DateTime end)
        {
            var result = new MayDenied<decimal?>(await CheckAdminRequest(shopDao, shopId, appKey));
            if (result.RequestResult == RequestResult.Success)
                result.Value = await shopDao.EvaluateSalesAsync(shopId, start, end);
            return result;
        }

        public async Task<Shop?> Get(Guid id)
        {
            return await shopDao.FindByIdAsync(id);
        }

        public async Task<IEnumerable<Shop>> GetAll()
        {
            return await shopDao.FindAllAsync();
        }

        public async Task<MayDenied<IEnumerable<Order>>> GetAllOrders(Guid shopId, string appKey, DateTime from, DateTime to)
        {
            var result = new MayDenied<IEnumerable<Order>>(await CheckAdminRequest(shopDao, shopId, appKey));
            if (result.RequestResult == RequestResult.Success)
                result.Value = await orderDao.FindAllForShopAsync(shopId, from, to);
            return result;
        }

        public async Task<IEnumerable<ProductAmount>> GetTopSellers(Guid shopId, int count, DateTime start, DateTime end)
        {
            return await shopDao.EvaluateTopsellersAsync(shopId, count, start, end);
        }

        public async Task Insert(Shop shop)
        {
            await shopDao.InsertAsync(shop);
        }

        public async Task<RequestResult> Update(Shop shop, string appKey)
        {
            var result = await CheckAdminRequest(shopDao, shop.Id, appKey);
            if(result == RequestResult.Success)
                await shopDao.UpdateAsync(shop);
            return result;
        }

        public async Task<Order?> GetOrder(Guid shopId, Guid id)
        {
            return await orderDao.FindByIdAsync(shopId, id);
        }

        public async Task<MayDenied<IEnumerable<Order>>> SeachOrders(Guid shopId, string appKey, DateTime from, DateTime to, string searchTerm)
        {
            var result = new MayDenied<IEnumerable<Order>>(await CheckAdminRequest(shopDao, shopId, appKey));
            if(result.RequestResult == RequestResult.Success)
                result.Value = await orderDao.SearchOrders(shopId, from, to, searchTerm);
            return result;
        }
    }
}
