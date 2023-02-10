using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DB;
using CaaS.Core.Domain;
using Microsoft.Extensions.Configuration;

namespace CaaS.Core.Tests
{
    [TestFixture]
    internal class ShopDaoTests
    {
        public class BasicOperationTests
        {
            private IConnectionFactory connectionFactory;
            private IShopDao sut;

            [OneTimeSetUp]
            public void Setup()
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "CaaSDbConnection");

                sut = new AdoShopDao(connectionFactory);
            }

            [SetUp]
            public void BeforeEach()
            {
                var manager = new DbManager(connectionFactory);
                manager.CreateAsync().Wait();
            }

            [TearDown]
            public void AfterEach()
            {
                var manager = new DbManager(connectionFactory);
                manager.DropAsync().Wait();
            }


            [Test]
            public void Insert_UpdatesShopId()
            {
                var shop = new Shop(Guid.Empty, "shop", "shopkey");

                sut.InsertAsync(shop).Wait();

                Assert.That(shop.Id, Is.Not.EqualTo(Guid.Empty));
            }

            [Test]
            public void FindAll_ReturnsAllShops()
            {
                var shops = new Shop[]
                {
                new Shop(Guid.Empty, "shop1", "shop1key"),
                new Shop(Guid.Empty, "shop2", "shop2key")
                };

                foreach (var shop in shops)
                    sut.InsertAsync(shop).Wait();

                var task = sut.FindAllAsync();
                task.Wait();

                var result = task.Result;

                CollectionAssert.AreEquivalent(result, shops);
            }

            [Test]
            public void FindById_ReturnsShop()
            {
                var shop = new Shop(Guid.Empty, "", "");
                sut.InsertAsync(shop).Wait();

                var task = sut.FindByIdAsync(shop.Id);
                task.Wait();
                var result = task.Result;

                Assert.That(result, Is.Not.Null);
                Assert.That(result, Is.EqualTo(shop));
            }

            [Test]
            public void UpdateWithValidShop_UpdatesEntry()
            {
                var shop = new Shop(Guid.Empty, "", "");
                sut.InsertAsync(shop).Wait();

                shop.Name = "shop";
                shop.AppKey = "key";

                sut.UpdateAsync(shop).Wait();

                var task = sut.FindByIdAsync(shop.Id);
                task.Wait();
                var result = task.Result;

                Assert.That(result, Is.EqualTo(shop));
            }


            [Test]
            public void DeleteWithValidShop_RemovesEntry()
            {
                var shop = new Shop(Guid.Empty, "", "");
                sut.InsertAsync(shop).Wait();

                shop.Name = "shop";
                shop.AppKey = "key";

                sut.DeleteAsync(shop.Id).Wait();

                var task = sut.FindByIdAsync(shop.Id);
                task.Wait();
                var result = task.Result;

                Assert.That(result, Is.Null);

            }
        }

        [TestFixture]
        public class AnalyticQueriesTests
        {
            private IConnectionFactory connectionFactory;
            private IShopDao sut;
            private IOrderDao orderDao;
            private IProductDao productDao;
            private ICartDao cartDao;
            private Shop shop;

            private static readonly Product[] products = new[] {
                new Product(Guid.Empty, "somePng", 3.33M, "some words",0, "some link"),
                new Product(Guid.Empty, "someJpg", 3.39M, "some words",1, "some link"),
                new Product(Guid.Empty, "somePdf", 1.78M, "some words",2, "some link"),
                new Product(Guid.Empty, "someExe", 5.99M, "some words",3, "some link")
            };

            private static readonly Cart[] carts = new[]
            {
                new Cart(Guid.Empty) {new ProductAmount(products[0], 3), new ProductAmount(products[3], 8)},
                new Cart(Guid.Empty) {new ProductAmount(products[2], 5) }
            };


            [OneTimeSetUp]
            public void Setup()
            {
                IConfiguration configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", optional: false)
                    .Build();

                connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "CaaSDbConnection");

                sut = new AdoShopDao(connectionFactory);
                orderDao = new AdoOrderDao(connectionFactory);
                productDao = new AdoProductDao(connectionFactory);
                cartDao = new AdoCartDao(connectionFactory);
            }

            [SetUp]
            public void BeforeEach()
            {
                var manager = new DbManager(connectionFactory);
                manager.CreateAsync().Wait();

                shop = new Shop(Guid.Empty, "shop", "key");
                sut.InsertAsync(shop).Wait();

                foreach (var product in products)
                    productDao.InsertAsync(shop.Id, product).Wait();

                foreach (var cart in carts)
                    cartDao.InsertAsync(shop.Id, cart).Wait();
            }

            [TearDown]
            public void AfterEach()
            {
                var manager = new DbManager(connectionFactory);
                manager.DropAsync().Wait();
            }

            [Test]
            public void EvaluateTopsellers_ReturnsExpected()
            {
                var order = new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[0]);
                orderDao.InsertAsync(shop.Id, order).Wait();

                var task = sut.EvaluateTopsellersAsync(shop.Id, 1, DateTime.Now.AddMinutes(-2), DateTime.Now);
                task.Wait();

                var result = task.Result.SingleOrDefault();
                var expected = carts[0][1];

                Assert.That(result, Is.Not.Null);
                Assert.Multiple(() =>
                {
                    Assert.That(result.Product, Is.EqualTo(expected.Product));
                    Assert.That(result, Has.Count.EqualTo(expected.Count));
                });
            }

            [Test]
            public void EvaluateSales_ReturnsExpectedValue()
            {
                var expected = carts.Sum(x => x.Sum(x => x.Product.Price * x.Count));

                foreach (var cart in carts)
                    orderDao.InsertAsync(shop.Id, new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), cart)).Wait();

                var task = sut.EvaluateSalesAsync(shop.Id, DateTime.Now.AddMinutes(-2), DateTime.Now);
                task.Wait();
                var result = task.Result;

                Assert.That(result, Is.EqualTo(expected));
            }


            [Test]
            public void EvaluateCartSalesDistributed_ReturnsValidDistribution()
            {
                var dt = new DateTime(2022, 11, 26);

                var orders = new Order[]
                {
                    new Order(Guid.Empty, dt.AddDays(-6), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-5), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt.AddDays(-4), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-3), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt.AddDays(-2), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-1), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt, new Customer("0", "0", "0"), carts[0]),
                };

                foreach (var order in orders)
                    orderDao.InsertAsync(shop.Id, order).Wait();

                var task = sut.EvaluateCartSalesDistributed(shop.Id, dt.AddDays(-6), dt);
                task.Wait();

                var result = task.Result;
                var cart1Sum = carts[0].Sum(x => x.Product.Price * x.Count);
                var cart2Sum = carts[1].Sum(x => x.Product.Price * x.Count);

                Assert.Multiple(() =>
                {
                    Assert.That(result.Sunday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Monday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Tuesday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Wednesday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Thursday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Friday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Saturday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                });
            }

            [Test]
            public void EvalueateCartProductCountDistributed_ReturnsValidDistribution()
            {
                var dt = new DateTime(2022, 11, 26);

                var orders = new Order[]
                {
                    new Order(Guid.Empty, dt.AddDays(-6), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-5), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt.AddDays(-4), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-3), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt.AddDays(-2), new Customer("0", "0", "0"), carts[0]),
                    new Order(Guid.Empty, dt.AddDays(-1), new Customer("1", "1", "1"), carts[1]),
                    new Order(Guid.Empty, dt, new Customer("0", "0", "0"), carts[0]),
                };

                foreach (var order in orders)
                    orderDao.InsertAsync(shop.Id, order).Wait();

                var task = sut.EvaluateCartProductCountDistributed(shop.Id, dt.AddDays(-6), dt);
                task.Wait();

                var result = task.Result;

                var cart1Sum = carts[0].Average(x => x.Count);
                var cart2Sum = carts[1].Average(x => x.Count);

                Assert.Multiple(() =>
                {
                    Assert.That(result.Sunday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Monday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Tuesday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Wednesday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Thursday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                    Assert.That(result.Friday.ToString("#.####"), Is.EqualTo(cart2Sum.ToString("#.####")));
                    Assert.That(result.Saturday.ToString("#.####"), Is.EqualTo(cart1Sum.ToString("#.####")));
                });
            }
        }
    }
}
