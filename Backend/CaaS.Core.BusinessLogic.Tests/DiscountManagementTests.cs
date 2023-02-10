using CaaS.Core.BusinessLogic.Common;
using CaaS.Core.BusinessLogic.Implementation;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Moq;
using System.Linq.Expressions;

namespace CaaS.Core.BusinessLogic.Tests
{
    [TestFixture]
    internal class DiscountManagementTests
    {
        private Mock<IShopDao> shopMock;
        private Mock<IDiscountDao> discountMock;

        private IDiscountManagementLogic sut;

        private readonly Shop shop = new (Guid.NewGuid(), "test", "test");

        [SetUp]
        public void BeforeEach()
        {
            shopMock = new Mock<IShopDao>();
            discountMock = new Mock<IDiscountDao>();
            sut = new DiscountManagementLogic(discountMock.Object, shopMock.Object);
        }

        private void PrepareShopFound()
        {
            discountMock.Setup(dm => dm.FindShopFromDiscountAsync(It.IsAny<Guid>()))
                .ReturnsAsync(shop);
            shopMock.Setup(sm => sm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(shop);
        }

        private void PrepareShopNotFound()
        {
            discountMock.Setup(dm => dm.FindShopFromDiscountAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Shop?)null);
            shopMock.Setup(dm => dm.FindByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Shop?)null);
        }

        [Test]
        public async Task DeleteWithValidInputs_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Delete(shop.Id, shop.AppKey, Guid.NewGuid());

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task DeleteWithValidInputs_CallsDelete()
        {
            PrepareShopFound();
            Expression<Action<IDiscountDao>> meth = dm => dm.DeleteAsync(It.IsAny<Guid>());
            discountMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, shop.AppKey, Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(1));
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
            Expression<Action<IDiscountDao>> meth = dm => dm.DeleteAsync(It.IsAny<Guid>());
            discountMock.Setup(meth).Verifiable();

            await sut.Delete(shop.Id, "", Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(0));
        }

        [Test]
        public async Task Get_CallsDaoFindById()
        {
            Expression<Action<IDiscountDao>> meth = dm => dm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>());

            discountMock.Setup(meth)
                .Verifiable();

            var result = await sut.Get(Guid.NewGuid(), Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetAll_CallsDaoFindAll()
        {
            Expression<Action<IDiscountDao>> meth = dm => dm.FindAllAsync(It.IsAny<Guid>());

            discountMock.Setup(meth)
                .Verifiable();

            var result = await sut.GetAll(Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task GetAllActive_CallsDaoFindAllAcrive()
        {
            Expression<Action<IDiscountDao>> meth = dm => dm.FindAllActiveAsync(It.IsAny<Guid>());

            discountMock.Setup(meth)
                .Verifiable();

            var result = await sut.GetAllActive(Guid.NewGuid());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task InsertWithValidInputs_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Insert(shop.Id, shop.AppKey, new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task InsertWithValidInputs_CallsInsert()
        {
            PrepareShopFound();
            Expression<Action<IDiscountDao>> meth = dm => dm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Discount>());
            discountMock.Setup(meth).Verifiable();

            await sut.Insert(shop.Id, shop.AppKey, new Discount());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task InsertWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Insert(shop.Id, shop.AppKey, new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task InsertWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Insert(shop.Id, "", new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task InsertWithInvalidInputs_DoesNotCallInsert()
        {
            PrepareShopFound();
            Expression<Action<IDiscountDao>> meth = dm => dm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Discount>());
            discountMock.Setup(meth).Verifiable();

            await sut.Insert(shop.Id, "", new Discount());

            discountMock.Verify(meth, Times.Exactly(0));
        }


        [Test]
        public async Task UpdateWithValidInputs_ReturnsSuccess()
        {
            PrepareShopFound();

            var result = await sut.Update(shop.Id, shop.AppKey, new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.Success));
        }

        [Test]
        public async Task UpdateWithValidInputs_CallsUpdate()
        {
            PrepareShopFound();
            Expression<Action<IDiscountDao>> meth = dm => dm.UpdateAsync(It.IsAny<Discount>());
            discountMock.Setup(meth).Verifiable();

            await sut.Update(shop.Id, shop.AppKey, new Discount());

            discountMock.Verify(meth, Times.Exactly(1));
        }

        [Test]
        public async Task UpdateWithInvalidShop_ReturnsFailure()
        {
            PrepareShopNotFound();

            var result = await sut.Update(shop.Id, shop.AppKey, new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.Failure));
        }

        [Test]
        public async Task UpdateWithInvalidAppKey_ReturnsNoPermission()
        {
            PrepareShopFound();

            var result = await sut.Update(shop.Id, "", new Discount());

            Assert.That(result, Is.EqualTo(RequestResult.NoPermission));
        }

        [Test]
        public async Task UpdateWithInvalidInputs_DoesNotCallUpdate()
        {
            PrepareShopFound();
            Expression<Action<IDiscountDao>> meth = dm => dm.UpdateAsync(It.IsAny<Discount>());
            discountMock.Setup(meth).Verifiable();

            await sut.Update(shop.Id, "", new Discount());

            discountMock.Verify(meth, Times.Exactly(0));
        }
    }
}
