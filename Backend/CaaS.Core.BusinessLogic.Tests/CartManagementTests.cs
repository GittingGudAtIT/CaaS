using CaaS.Core.BusinessLogic.Implementation;
using CaaS.Core.BusinessLogic.Interface;
using CaaS.Core.Dal.Interface;
using CaaS.Core.Domain;
using Moq;
using System.Drawing.Printing;
using System.Linq.Expressions;

namespace CaaS.Core.BusinessLogic.Tests
{
    [TestFixture]
    public class CartManagementTests
    {
        private Mock<ICartDao> cartMock;
        private Mock<IDiscountDao> discountMock;
        private Mock<IOrderDao> orderMock;

        private ICartManagementLogic sut;


        private static readonly ProductAmount[] cartEntries = new[]
        {
            new ProductAmount(new Product(
                Guid.NewGuid(),
                "prod1",
                3.3M,
                "description1",
                0,
                ""
            ), 5),
            new ProductAmount(new Product(
                Guid.NewGuid(),
                "prod2",
                3.3M,
                "description2",
                1,
                ""
            ), 3)
        };


        private readonly DiscountLookup[] prodDiscounts = new []
        {
            new DiscountLookup(
                new Discount(Guid.NewGuid(), OffType.FreeProduct, 1, "", "", MinType.ProductCount, 3, true, DateTime.Now, DateTime.Now.AddDays(1)), 
                cartEntries.Select(x => x.Product.Id)
            ),
            new DiscountLookup(
                new Discount(Guid.NewGuid(), OffType.Percentual, 0.2M, "", "", MinType.ProductCount, 3, true, DateTime.Now, DateTime.Now.AddDays(1)),
                cartEntries.Select(x => x.Product.Id)
            ),
        };
        private readonly Discount[] valDiscounts = new[]
        {
            new Discount(Guid.NewGuid(), OffType.Percentual, 0.3M, "", "", MinType.CartSum, 10, false, DateTime.Now, DateTime.Now.AddDays(1)),
            new Discount(Guid.NewGuid(), OffType.None, 0M, "", "", MinType.CartSum, 10, false, DateTime.Now, DateTime.Now.AddDays(1),cartEntries),
        };

        [SetUp]
        public void BeforeEach()
        {
            // prepare moqs
            cartMock = new Mock<ICartDao>();
            discountMock = new Mock<IDiscountDao>();
            orderMock = new Mock<IOrderDao>();

            sut = new CartManagementLogic(
                cartMock.Object, 
                discountMock.Object, 
                orderMock.Object
            );
        }

        [Test]
        public async Task GetWithValidIds_ReturnsValidCart()
        {
            var cart = new Cart(Guid.NewGuid(), cartEntries);
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(cart).Verifiable();


            var result = await sut.Get(Guid.NewGuid(), Guid.NewGuid());


            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Id, Is.EqualTo(cart.Id));
                CollectionAssert.AreEqual(cartEntries, result);
                cartMock.Verify(
                    cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                    Times.Exactly(1)
                );
            });
        }

        [Test]
        public async Task Add_UpdatedsCartId()
        {
            var cart = new Cart(Guid.Empty, cartEntries);
            cartMock.Setup(cm => cm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Cart>()))
                .Callback(() => cart.Id = Guid.NewGuid());


            await sut.Add(Guid.NewGuid(), cart);


            Assert.Multiple(() =>
            {
                Assert.That(cart.Id, Is.Not.EqualTo(Guid.Empty));
                cartMock.Verify(
                    cm => cm.InsertAsync(It.IsAny<Guid>(), It.IsAny<Cart>()),
                    Times.Exactly(1)
                );
            });
        }

        [Test]
        public async Task Delete_CallsDaoDelete()
        {
            cartMock.Setup(cm => cm.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .Verifiable();

            await sut.Delete(Guid.NewGuid(), Guid.NewGuid());

            cartMock.Verify(
                cm => cm.DeleteAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Exactly(1)
            );
        }

        [Test]
        public async Task GetDiscounts_ReturnsValidCartDiscounts()
        {
            discountMock.Setup(dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(prodDiscounts)
                .Verifiable();
            discountMock.Setup(dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(valDiscounts)
                .Verifiable();

            var result = await sut.GetDiscounts(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.ProductDiscounts, Is.EqualTo(prodDiscounts));
                Assert.That(result.ValueDiscounts, Is.EqualTo(valDiscounts));
                discountMock.Verify(
                    dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()),
                    Times.Exactly(1)
                );
                discountMock.Verify(
                    dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                    Times.Exactly(1)
                );
            });
        }

        [Test]
        public async Task GetSum_WithNonExistingCart_ReturnsZero()
        {
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Cart?)null);
            discountMock.Setup(dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<DiscountLookup>());
            discountMock.Setup(dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<Discount>());

            var result = await sut.GetSum(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.Zero);
        }

        [Test]
        public async Task GetSum_WithExistingCart_ReturnsExpectedValue()
        {
            var cart = new Cart(Guid.NewGuid(), cartEntries);
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(cart);
            discountMock.Setup(dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(prodDiscounts);
            discountMock.Setup(dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(valDiscounts);

            var result = await sut.GetSum(Guid.NewGuid(), Guid.NewGuid());
            cart.CartDiscounts = new(prodDiscounts, valDiscounts);

            Assert.That(result, Is.EqualTo(cart.EvaluateAmount()));
        }

        [Test]
        public async Task GetSum_CallsAllNeededMethods()
        {
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Cart(Guid.NewGuid()))
                .Verifiable();
            discountMock.Setup(dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<DiscountLookup>())
                .Verifiable();
            discountMock.Setup(dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(Enumerable.Empty<Discount>())
                .Verifiable();

            var result = await sut.GetSum(Guid.NewGuid(), Guid.NewGuid());

            cartMock.Verify(
                cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Exactly(1)
            );
            discountMock.Verify(
                dm => dm.FindProductDiscountsForCart(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Exactly(1)
            );
            discountMock.Verify(
                dm => dm.FindValueDiscountsForCartAsync(It.IsAny<Guid>(), It.IsAny<Guid>()),
                Times.Exactly(1)
            );
        }

        [Test]
        public async Task Update_CallsDaoUpdate()
        {
            cartMock.Setup(cm => cm.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Cart>()))
                .Verifiable();

            await sut.Update(Guid.NewGuid(), new Cart(Guid.NewGuid()));

            cartMock.Verify(
                cm => cm.UpdateAsync(It.IsAny<Guid>(), It.IsAny<Cart>()),
                Times.Exactly(1)
            );
        }

        [Test]
        public async Task CheckoutWithValidCart_ReturnsValidOrder()
        {
            var cart = new Cart(Guid.NewGuid(), cartEntries);
            var customer = new Customer("", "", "");
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(cart);
            orderMock.Setup(om => om.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync(new Order(Guid.NewGuid(), DateTime.Now, customer, cart));

            var result = await sut.Checkout(Guid.NewGuid(), Guid.NewGuid(), customer);


            Assert.That(result, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(result.Customer, Is.EqualTo(customer));
                CollectionAssert.AreEquivalent(
                    result.Entries.Select(x => x.Product.OriginalId),
                    cartEntries.Select(x => x.Product.Id)
                );
            });
        }

        [Test]
        public async Task CheckoutWithEmptyCart_DoesNotCallInsert()
        {
            var customer = new Customer("", "", "");
            cartMock.Setup(cm => cm.FindByIdAsync(It.IsAny<Guid>(), It.IsAny<Guid>()))
                .ReturnsAsync((Cart?)null);

            orderMock.Setup(om => om.InsertAsync(It.IsAny<Guid>(), It.IsAny<Order>()))
                .Verifiable();

            var result = await sut.Checkout(Guid.NewGuid(), Guid.NewGuid(), customer);

            Assert.That(result, Is.Null);
            orderMock.Verify(
                om => om.InsertAsync(It.IsAny<Guid>(), It.IsAny<Order>()),
                Times.Never()
            );
        }
    }
}