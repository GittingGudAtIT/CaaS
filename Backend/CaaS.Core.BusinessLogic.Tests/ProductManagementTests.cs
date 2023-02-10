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
    public class ProductManagementTests
    {
        private Mock<IShopDao> shopMock;
        private Mock<IDiscountDao> discountMock;
        private Mock<IProductDao> productMock;

        private IProductManagementLogic sut;

        private readonly Shop shop = new(Guid.NewGuid(), "test", "test");

        [SetUp]
        public void BeforeEach()
        {
            shopMock = new Mock<IShopDao>();
            discountMock = new Mock<IDiscountDao>();
            productMock = new Mock<IProductDao>();
            sut = new ProductManagementLogic(productMock.Object, discountMock.Object, shopMock.Object);
        }

        private void PrepareShopFound()
        {
            productMock.Setup(dm => dm.FindShopFromProductAsync(It.IsAny<Guid>()))
                .ReturnsAsync(shop);
            shopMock.Setup(sm => sm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(shop);
        }

        private void PrepareShopNotFound()
        {
            productMock.Setup(dm => dm.FindShopFromProductAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Shop?)null);
            shopMock.Setup(dm => dm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Shop?)null);
        }

        [Test]
        public async Task GetWithSingleId_CallsDaoFindById()
        {
            Expression<Action<IProductDao>> meth = pm => pm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>());
            productMock.Setup(meth).Verifiable();

            await sut.Get(Guid.NewGuid(), Guid.NewGuid());

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetWithMultibleIds_CallsDaoFindById()
        {
            Expression<Action<IProductDao>> meth = pm => pm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>());
            productMock.Setup(meth).Verifiable();

            await sut.Get(Guid.NewGuid(), Enumerable.Empty<Guid>());

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetAll_CallsDaoFindAll()
        {
            Expression<Action<IProductDao>> meth = pm => pm.FindAllAsync(It.IsAny<Guid>());
            productMock.Setup(meth).Verifiable();

            await sut.GetAll(Guid.NewGuid());

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetDiscountsForProductSingle_CallsDaoDiscountsForProductSingle()
        {
            Expression<Action<IDiscountDao>> meth = dm => dm.FindForProductAsync(It.IsAny<Guid>(), It.IsAny<Guid>());
            discountMock.Setup(meth).Verifiable();

            await sut.GetDiscountsForProduct(Guid.NewGuid(), Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetDiscountsForProductMultible_CallsDaoDiscountsForProductMultible()
        {
            Expression<Action<IDiscountDao>> meth = dm => dm.FindForProductsAsync(It.IsAny<Guid>(), It.IsAny<IEnumerable<Guid>>());
            discountMock.Setup(meth).Verifiable();

            await sut.GetDiscountsForProducts(Guid.NewGuid(), Enumerable.Empty<Guid>());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task Search_CallsDaoSearch()
        {
            Expression<Action<IProductDao>> meth = pm => pm.SearchAsync(It.IsAny<Guid>(), It.IsAny<string>());
            productMock.Setup(meth).Verifiable();

            await sut.Search(Guid.NewGuid(), "");

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task UpdateWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Update(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "", 0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task UpdateWithValidInput_CallsUpdate()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.UpdateAsync(It.IsAny<Product>());
            productMock.Setup(meth).Verifiable();

            await sut.Update(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "",0, ""));

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task UpdateWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Update(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "",0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task UpdateWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Update(shop.Id, "", new Product(Guid.Empty, "", 0, "",0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task UpdateWithInvalidInputs_DoesNotCallUpdate()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.UpdateAsync(It.IsAny<Product>());
            productMock.Setup(meth).Verifiable();

            await sut.Update(shop.Id, "", new Product(Guid.Empty, "", 0, "",0, ""));

            productMock.Verify(meth, Times.Never());
        }

        [Test]
        public async Task InsertWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Insert(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "",0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task InsertWithValidInput_CallsInsert()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Product>());
            productMock.Setup(meth).Verifiable();

            await sut.Insert(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "",0, ""));

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task InsertWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Insert(shop.Id, shop.AppKey, new Product(Guid.Empty, "", 0, "",0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task InsertWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Insert(shop.Id, "", new Product(Guid.Empty, "", 0, "",0, ""));

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task InsertWithInvalidInputs_DoesNotCallInsert()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Product>());
            productMock.Setup(meth).Verifiable();

            await sut.Insert(shop.Id, "", new Product(Guid.Empty, "", 0, "",0, ""));

            productMock.Verify(meth, Times.Never());
        }

        [Test]
        public async Task DeleteWithValidInput_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Delete(shop.Id, shop.AppKey, Guid.NewGuid());

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task DeleteWithValidInput_CallsDelete()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.DeleteAsync(It.IsAny<Guid>());
            productMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, shop.AppKey, Guid.NewGuid());

            productMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task DeleteWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Delete(shop.Id, shop.AppKey, Guid.NewGuid());

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task DeleteWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Delete(shop.Id, "", Guid.NewGuid());

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task DeleteWithInvalidInputs_DoesNotCallDelete()
        {
            PrepareShopFound();
            Expression<Action<IProductDao>> meth = pm => pm.DeleteAsync(It.IsAny<Guid>());
            productMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, "", Guid.NewGuid());

            productMock.Verify(meth, Times.Never());
        }
    }
}
