using Azure;
using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DB;
using CaaS.Core.DBLayer.Domain.TestExtensions;
using CaaS.Core.Domain;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace CaaS.Core.Tests
{
    [TestFixture]
    public class DiscountDaoTests
    {
        private IConnectionFactory connectionFactory;
        private IDiscountDao sut;
        private readonly Shop shop = new(Guid.Empty, "shop1", "shopkey");
        private static readonly Product[] products = new[]
        {
            new Product(Guid.Empty, "prod0", 3.33M, "some words", 0, "some link"),
            new Product(Guid.Empty, "prod1", 9.99M, "some words", 1, "some link"),
            new Product(Guid.Empty, "prod2", 1.82M, "some words", 2, "some link"),
            new Product(Guid.Empty, "prod3", 5.25M, "some words", 3, "some link"),
        };

        [OneTimeSetUp]
        public void Setup()
        {
            IConfiguration configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false)
            .Build();

            connectionFactory = DefaultConnectionFactory.FromConfiguration(configuration, "CaaSDbConnection");

            var manager = new DbManager(connectionFactory);
            manager.CreateAsync().Wait();

            sut = new AdoDiscountDao(connectionFactory);
        }

        [SetUp]
        public void BeforeEach()
        {
            var manager = new DbManager(connectionFactory);
            manager.CreateAsync().Wait();

            var shopDao = new AdoShopDao(connectionFactory);
            var productDao = new AdoProductDao(connectionFactory);

            shopDao.InsertAsync(shop).Wait();
            foreach (var product in products)
                productDao.InsertAsync(shop.Id, product).Wait();
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            var manager = new DbManager(connectionFactory);
            manager.DropAsync().Wait();
        }

        [Test]
        public void Insert_UpdatesId()
        {
            var discounts = new Discount[]
            {
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1)),
                new Discount(Guid.Empty, OffType.FreeProduct, 0M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1), new ProductAmount[]
                {
                    new (products[0], 3),
                    new (products[1], 1),
                })
            };
            foreach (var source in discounts)
                sut.InsertAsync(shop.Id, source).Wait();

            Assert.Multiple(() =>
            {
                Assert.That(discounts[0].Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(discounts[1].Id, Is.Not.EqualTo(Guid.Empty));
            });
        }

        [Test]
        public void FindByIdWithValidId_ReturnsValidDiscount()
        {
            var discount = new Discount(Guid.Empty, OffType.FreeProduct, 0M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1), new ProductAmount[]
            {
                new (products[0], 3),
                new (products[1], 1),
            });
            
            sut.InsertAsync(shop.Id, discount).Wait();

            var task = sut.FindByIdAsync(shop.Id, discount.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.FullyEquals(discount), Is.True);
        }

        [Test]
        public void InsertProducts_ResultsInProductsAreDiscountedNow()
        {
            var discount = new Discount(Guid.Empty, OffType.Percentual, 0.3M, "", "", MinType.ProductCount, 3M, false, DateTime.Now, DateTime.Now.AddDays(1), null, products.Select(x => x.Id));
            sut.InsertAsync(shop.Id, discount).Wait();

            using var conTask = connectionFactory.CreateConnectionAsync();
            conTask.Wait();
            using DbConnection connection = conTask.Result;
            using DbCommand command = connection.CreateCommand();

            command.CommandText = "select * from productdiscountproduct";
            using DbDataReader reader = command.ExecuteReader();

            var productIds = new List<Guid>();
            while (reader.Read())
                productIds.Add((Guid)reader["productid"]);

            CollectionAssert.AreEquivalent(products.Select(x => x.Id), productIds);
        }


        [Test]
        public void FindAll_ReturnsValidSetOfDiscounts()
        {
            var discounts = new Discount[]
            {
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1)),
                new Discount(Guid.Empty, OffType.FreeProduct, 0M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1), new ProductAmount[]
                {
                    new (products[0], 3),
                    new (products[1], 1),
                })
            };

            foreach (var discount in discounts)
                sut.InsertAsync(shop.Id, discount).Wait();


            var task = sut.FindAllAsync(shop.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.All(x => discounts.Where(y => y.FullyEquals(x)).Count() == 1), Is.True);
        }

        [Test]
        public void FindDiscountsForProductsWhenDiscountIsForAllProducts_ReturnsExpectedDiscounts()
        {
            var discount = new Discount(Guid.Empty, OffType.Percentual, 0.3M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1));
            sut.InsertAsync(shop.Id, discount).Wait();

            var task = sut.FindForProductsAsync(shop.Id, products.Select(x => x.Id));
            task.Wait();
            var result = task.Result;

            Assert.That(result.Count(), Is.EqualTo(1));
            CollectionAssert.AreEquivalent(result.First().ProductIds, products.Select(x => x.Id));
        }

        [Test]
        public void FindDiscounsForProductsWhenDiscountIsForSpecific_ReturnsExpectedDiscounts()
        {
            var discount = new Discount(Guid.Empty, OffType.Percentual, 0.3M, "", "", MinType.ProductCount, 3M, false, DateTime.Now, DateTime.Now.AddDays(1), null, products.Skip(1).Select(x => x.Id));
            sut.InsertAsync(shop.Id, discount).Wait();

            var task = sut.FindForProductsAsync(shop.Id, products.Select(x => x.Id));
            task.Wait();
            var result = task.Result;

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                CollectionAssert.AreEquivalent(products.Skip(1).Select(x => x.Id), result.First().ProductIds);
                CollectionAssert.DoesNotContain(result.First().ProductIds, products.First().Id);
            });
        }

        [Test]
        public void Update_ReturnsExpectedObject()
        {
            var discount = new Discount(Guid.Empty, OffType.FreeProduct, 0.3M, "", "", MinType.ProductCount, 3M, true, DateTime.Now, DateTime.Now.AddDays(1),
                new List<ProductAmount> { new (products[0], 3) }
            );
            sut.InsertAsync(shop.Id, discount).Wait();
            discount.MinValue = 2M;
            ((List<ProductAmount>)discount.FreeProducts).Add(new (products[1], 1));

            sut.UpdateAsync(discount).Wait();

            var task = sut.FindByIdAsync(shop.Id, discount.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Not.Null);

            new JsonSerializer().Serialize(Console.Out, discount);
            Console.WriteLine();
            new JsonSerializer().Serialize(Console.Out, result);
            Assert.That(result.FullyEquals(discount), Is.True);
        }

        [Test]
        public void Delete_RemovesEntryAndAllConnections()
        {
            var discount = new Discount(Guid.Empty, OffType.FreeProduct, 0.3M, "", "", MinType.ProductCount, 3M, false, DateTime.Now, DateTime.Now.AddDays(1),
                new List<ProductAmount> { new (products[0], 3) }, products.Select(x => x.Id)
            );
            sut.InsertAsync(shop.Id, discount).Wait();
            sut.DeleteAsync(discount.Id).Wait();

            var task = sut.FindByIdAsync(shop.Id, discount.Id);
            task.Wait();
            var result = task.Result;

            using var conTask = connectionFactory.CreateConnectionAsync();
            conTask.Wait();
            using var connection = conTask.Result;
            using var command = connection.CreateCommand();

            command.CommandText =
                "select  (select count(*) from productdiscountproduct) + " +
                        "(select count(*) from discountfreeproduct)";

            var count = (int)command.ExecuteScalar()!;

            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Null);
                Assert.That(count, Is.Zero);
            });

        }

        [Test]
        public void FindValueDiscountsForCart_ReturnsExpectedDiscounts()
        {
            var discounts = new Discount[]
            {
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.CartSum, 10, true, DateTime.Now, DateTime.Now.AddDays(1),
                    new ProductAmount[]{ new (products[3], 1) }),
                new Discount(Guid.Empty, OffType.Fixed, 1, "", "", MinType.CartSum, 10, true, DateTime.Now, DateTime.Now.AddDays(1))
            };

            foreach (var discount in discounts)
                sut.InsertAsync(shop.Id, discount).Wait();

            var cart = new Cart(Guid.Empty) { new ProductAmount(products[0], 3), new ProductAmount(products[1], 1) };
            var cartDao = new AdoCartDao(connectionFactory);
            cartDao.InsertAsync(shop.Id, cart).Wait();

            var task = sut.FindValueDiscountsForCartAsync(shop.Id, cart.Id);
            task.Wait();
            var result = task.Result;

            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.All(x => discounts.Where(y => y.FullyEquals(x)).Count() == 1), Is.True);
            });
        }

        [Test]
        public void FindShopForProduct_ReturnsValidShop()
        {
            var discount = new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.CartSum, 10, true, DateTime.Now, DateTime.Now.AddDays(1),
                new ProductAmount[] { new (products[3], 1) }
            );
            sut.InsertAsync(shop.Id, discount).Wait();

            var task = sut.FindShopFromDiscountAsync(discount.Id);
            task.Wait();

            var result = task.Result;

            Assert.That(result?.FullyEquals(shop), Is.True);
        }

        [Test]
        public void FindAllActive_ReturnsExpectedDiscounts()
        {
            var discounts = new Discount[]
            {
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.CartSum, 10, true, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1),
                    new ProductAmount[] { new(products[1], 1)}
                ),
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.CartSum, 10, true, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)),
                new Discount(Guid.Empty, OffType.Percentual, 0.2M, "", "", MinType.CartSum, 10, true, DateTime.Now.AddHours(1), DateTime.Now.AddDays(1))
            };

            foreach (var discount in discounts)
                sut.InsertAsync(shop.Id, discount).Wait();

            var task = sut.FindAllActiveAsync(shop.Id);
            task.Wait();

            var result = task.Result;

            Assert.Multiple(() =>
            {
                CollectionAssert.Contains(result, discounts[0]);
                CollectionAssert.Contains(result, discounts[1]);
                CollectionAssert.DoesNotContain(result, discounts[2]);
            });
        }

        [Test]
        public void FindAllProductDiscountsForCart_ReturnsExpectedDiscounts()
        {
            var cart = new Cart(Guid.Empty, new[] {
                new ProductAmount(products[0], 5),
                new ProductAmount(products[1], 3),
                new ProductAmount(products[2], 5),
                new ProductAmount(products[3], 1),
            });
            var cartDao = new AdoCartDao(connectionFactory);
            cartDao.InsertAsync(shop.Id, cart).Wait();

            var discounts = new Discount[]
            {
                new Discount(Guid.Empty, OffType.FreeProduct, 1, "", "", MinType.ProductCount, 3, true, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)),
                new Discount(Guid.Empty, OffType.FreeProduct, 1, "", "", MinType.ProductCount, 3, false, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1), null, new Guid[]{
                    products[0].Id, products[1].Id
                }), // not for product with index 2
                new Discount(Guid.Empty, OffType.FreeProduct, 1, "", "", MinType.ProductCount, 5, true, DateTime.Now.AddDays(-1), DateTime.Now.AddDays(1)),
                new Discount(Guid.Empty, OffType.FreeProduct, 1, "", "", MinType.ProductCount, 5, true, DateTime.Now.AddDays(-2), DateTime.Now.AddDays(-1)) // not today
            };

            foreach (var discount in discounts)
                sut.InsertAsync(shop.Id, discount).Wait();


            var task = sut.FindProductDiscountsForCart(shop.Id, cart.Id);
            task.Wait();
            var result = task.Result;

            Assert.Multiple(() =>
            {
                CollectionAssert.IsNotEmpty(result);
                CollectionAssert.DoesNotContain(result.Select(x => x.Discount), discounts[3]);
                CollectionAssert.AreEquivalent(discounts.SkipLast(1), result.Select(x => x.Discount));

                var idsDis0 = result.Where(x => x.Discount.Equals(discounts[0])).First().ProductIds;
                var idsDis1 = result.Where(x => x.Discount.Equals(discounts[1])).First().ProductIds;
                var idsDis2 = result.Where(x => x.Discount.Equals(discounts[2])).First().ProductIds;

                CollectionAssert.AreEquivalent(new Guid[] { products[0].Id, products[1].Id, products[2].Id }, idsDis0);
                CollectionAssert.AreEquivalent(new Guid[] { products[0].Id, products[1].Id }, idsDis1);
                CollectionAssert.AreEquivalent(new Guid[] { products[0].Id, products[2].Id }, idsDis2);
            });

        }
    }
}
