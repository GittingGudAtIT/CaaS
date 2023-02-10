using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Implementation;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.BusinessLogic.Tests
{
    [TestFixture]
    public class ShopManagementTests
    {
        private Mock<IShopDao> shopMock;
        private Mock<IOrderDao> orderMock;

        private IShopManagementLogic sut;

        private readonly Shop shop = new(Guid.NewGuid(), "test", "test");

        [SetUp]
        public void BeforeEach()
        {
            shopMock = new Mock<IShopDao>();
            orderMock = new Mock<IOrderDao>();
            sut = new ShopManagementLogic(shopMock.Object, orderMock.Object);
        }

        private void PrepareShopFound()
        {
            shopMock.Setup(sm => sm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(shop);
        }

        private void PrepareShopNotFound()
        {
            shopMock.Setup(dm => dm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Shop?)null);
        }

        [Test]
        public async Task Get_CallsDaoFindById()
        {
            Expression<Action<IShopDao>> meth = sm => sm.FindByIdAsync(It.IsAny<Guid>());
            shopMock.Setup(meth).Verifiable();

            await sut.Get(Guid.NewGuid());

            shopMock.Verify(meth, Times.Exactly(1));
        }


        [Test]
        public async Task Insert_CallsDaoInsert()
        {
            Expression<Action<IShopDao>> meth = sm => sm.InsertAsync(It.IsAny<Shop>());
            shopMock.Setup(meth).Verifiable();

            await sut.Insert(new Shop());

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetAll_CallsDaoGetAll()
        {
            Expression<Action<IShopDao>> meth = sm => sm.FindAllAsync();
            shopMock.Setup(meth).Verifiable();

            await sut.GetAll();

            shopMock.Verify(meth, Times.Exactly(1));
        }


        [Test]
        public async Task GetTopSellers_CallsDaoGetTopSellers()
        {
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateTopsellersAsync(
                It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.GetTopSellers(Guid.NewGuid() ,0, DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetOrder_CallsDaoGetOrder()
        {
            Expression<Action<IOrderDao>> meth = om => om.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>());
            orderMock.Setup(meth).Verifiable();

            await sut.GetOrder(Guid.NewGuid(), Guid.NewGuid());

            orderMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task DeleteWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Delete(shop.Id, shop.AppKey);

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task DeleteWithValidInput_CallsDelete()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.DeleteAsync(It.IsAny<Guid>());
            shopMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, shop.AppKey);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task DeleteWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Delete(shop.Id, shop.AppKey);

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task DeleteWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Delete(shop.Id, "");

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task DeleteWithInvalidInputs_DoesNotCallDelete()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = pm => pm.DeleteAsync(It.IsAny<Guid>());
            shopMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, "");

            shopMock.Verify(meth, Times.Never());
        }


        [Test]
        public async Task UpdateWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Update(shop, shop.AppKey);

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task UpdateWithValidInput_CallsUpdate()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.UpdateAsync(It.IsAny<Shop>());
            shopMock.Setup(meth).Verifiable();

            await sut.Update(shop, shop.AppKey);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task UpdateWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Update(shop, shop.AppKey);

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task UpdateWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Update(shop, "");

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task UpdateWithInvalidInputs_DoesNotCallUpdate()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = pm => pm.UpdateAsync(It.IsAny<Shop>());
            shopMock.Setup(meth).Verifiable();

            await sut.Update(shop, "");

            shopMock.Verify(meth, Times.Never());
        }


        [Test]
        public async Task EvaluateSalesWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.EvaluateSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task EvaluateSalesWithValidInput_CallsEvaluateSales()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateSalesAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task EvaluateSalesWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.EvaluateSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task EvaluateSalesWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.EvaluateSales(shop.Id, "", DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task EvaluateSalesWithInvalidInputs_DoesNotCallEvaluateSales()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateSalesAsync(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateSales(shop.Id, "", DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Never());
        }


        [Test]
        public async Task EvaluateCartSalesWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.EvaluateCartSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task EvaluateCartSalesWithValidInput_CallsEvaluateCartSales()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateCartSalesDistributed(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateCartSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task EvaluateCartSalesWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.EvaluateCartSales(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task EvaluateCartSalesWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.EvaluateCartSales(shop.Id, "", DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task EvaluateCartSalesWithInvalidInputs_DoesNotCallEvaluateCartSales()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateCartSalesDistributed(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateCartSales(shop.Id, "", DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Never());
        }

        [Test]
        public async Task EvaluateCartCountsWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.EvaluateCartCounts(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task EvaluateCartCountsWithValidInput_CallsEvaluateCartCounts()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateCartProductCountDistributed(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateCartCounts(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task EvaluateCartCountsWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.EvaluateCartCounts(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task EvaluateCartCountsWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.EvaluateCartCounts(shop.Id, "", DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task EvaluateCartCountsWithInvalidInputs_DoesNotCallEvaluateCartCounts()
        {
            PrepareShopFound();
            Expression<Action<IShopDao>> meth = sm => sm.EvaluateCartProductCountDistributed(
                It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>()
            );
            shopMock.Setup(meth).Verifiable();

            await sut.EvaluateCartCounts(shop.Id, "", DateTime.Now, DateTime.Now);

            shopMock.Verify(meth, Times.Never());
        }

        [Test]
        public async Task GetAllOrdersWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.GetAllOrders(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task GetAllOrdersWithValidInput_CallsGetAllOrders()
        {
            PrepareShopFound();
            Expression<Action<IOrderDao>> meth = om => om.FindAllForShopAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());
            orderMock.Setup(meth).Verifiable();

            await sut.GetAllOrders(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            orderMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetAllOrdersWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.GetAllOrders(shop.Id, shop.AppKey, DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task GetAllOrdersWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.GetAllOrders(shop.Id, "", DateTime.Now, DateTime.Now);

            Assert.That(result.RequestResult, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task GetAllOrdersWithInvalidInputs_DoesNotCallGetAllOrders()
        {
            PrepareShopFound();
            Expression<Action<IOrderDao>> meth = om => om.FindAllForShopAsync(It.IsAny<Guid>(), It.IsAny<DateTime>(), It.IsAny<DateTime>());
            orderMock.Setup(meth).Verifiable();

            await sut.GetAllOrders(shop.Id, "", DateTime.Now, DateTime.Now);

            orderMock.Verify(meth, Times.Never());
        }

    }
}
