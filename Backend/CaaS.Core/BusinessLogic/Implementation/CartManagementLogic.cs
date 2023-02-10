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
    public class CartManagementLogic : ICartManagementLogic
    {
        private readonly ICartDao cartDao;
        private readonly IDiscountDao discountDao;
        private readonly IOrderDao orderDao;

        public CartManagementLogic(ICartDao cartDao, IDiscountDao discountDao, IOrderDao orderDao)
        {
            this.cartDao = cartDao;
            this.discountDao = discountDao;
            this.orderDao = orderDao;
        }

        public CartManagementLogic()
        {
            var connectionFactory = Connection.GetFactory();
            cartDao = new AdoCartDao(connectionFactory);
            discountDao = new AdoDiscountDao(connectionFactory);
            orderDao = new AdoOrderDao(connectionFactory);
        }


        public async Task Add(Guid shopId, Cart cart)
        {
            await cartDao.InsertAsync(shopId, cart);
        }

        public async Task<Order?> Checkout(Guid shopId, Guid id, Customer customer)
        {
            var cart = await cartDao.FindByIdAsync(shopId, id);

            if(cart != null)
            {
                cart.CartDiscounts = new CartDiscounts(
                    await discountDao.FindProductDiscountsForCart(shopId, id),
                    await discountDao.FindValueDiscountsForCartAsync(shopId, id)
                );
                var order = new Order(Guid.Empty, DateTime.Now, customer, cart);
                await cartDao.DeleteAsync(shopId, id);
                await orderDao.InsertAsync(shopId, order);

                return order;
            }
            return null;
        }

        public async Task Delete(Guid shopId, Guid id)
        {
            await cartDao.DeleteAsync(shopId, id);
        }

        public async Task<Cart?> Get(Guid shopId, Guid id)
        {
            return await cartDao.FindByIdAsync(shopId, id);
        }

        public async Task<CartDiscounts> GetDiscounts(Guid shopId, Guid cartId)
        {
            var prodTask = discountDao.FindProductDiscountsForCart(shopId, cartId);
            var valTask = discountDao.FindValueDiscountsForCartAsync(shopId, cartId);

            await Task.WhenAll(prodTask, valTask);
            return new(
                prodTask.Result,
                valTask.Result
            );
        }

        public async Task<decimal> GetSum(Guid shopId, Guid id)
        {
            var cart = await cartDao.FindByIdAsync(shopId, id);

            if(cart == null)
                return 0;

            cart.CartDiscounts = await GetDiscounts(shopId, id);
            return cart.EvaluateAmount();
        }

        public async Task Update(Guid shopId, Cart cart)
        {
            await cartDao.UpdateAsync(shopId, cart);
        }
    }
}
