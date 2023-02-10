using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DB;
using CaaS.Core.DBLayer.Domain.TestExtensions;
using CaaS.Core.Domain;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using NUnit.Framework.Constraints;
using Org.BouncyCastle.Crypto.Modes.Gcm;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CaaS.Core.Tests
{
    [TestFixture]
    internal class OrderDaoTests
    {
        private IConnectionFactory connectionFactory;
        private IOrderDao sut;
        private Shop shop = new(Guid.Empty, "shop1", "shopkey");
        private readonly static Product[] products = new[]
        {
            new Product(Guid.Empty, "p1", 9.9M, "p1",0, "dlp1"),
            new Product(Guid.Empty, "p2", 3.8M, "p2",1, "dlp2"),
            new Product(Guid.Empty, "p3", 1.27M, "p3",2, "dlp3"),
            new Product(Guid.Empty, "p4", 8.99M, "p4",3, "dlp4")
        };

        private readonly static Cart[] carts = new[]
        {
            new Cart(Guid.Empty){ new ProductAmount(products[0], 3), new ProductAmount(products[2], 2)},
            new Cart(Guid.Empty){ new ProductAmount(products[1], 1), new ProductAmount(products[3], 5)}
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

            var shopDao = new AdoShopDao(connectionFactory);
            shopDao.InsertAsync(shop).Wait();

            var productDao = new AdoProductDao(connectionFactory);
            foreach (var prodct in products)
                productDao.InsertAsync(shop.Id, prodct).Wait();

            var cartDao = new AdoCartDao(connectionFactory);
            foreach(var cart in carts)
                cartDao.InsertAsync(shop.Id, cart).Wait();

            sut = new AdoOrderDao(connectionFactory);
        }

        [TearDown]
        public void AfterEach()
        {
            var task = connectionFactory.CreateConnectionAsync();
            task.Wait();

            using DbConnection connection = task.Result;
            using DbCommand command = connection.CreateCommand();

            command.CommandText = "delete from [order]";
            command.ExecuteNonQuery();
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            var manager = new DbManager(connectionFactory);
            manager.DropAsync().Wait();
        }

        [Test]
        public void Insert_UpdatesOrderId()
        {
            var order = new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[0]);

            sut.InsertAsync(shop.Id, order).Wait();

            Assert.That(order.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void FindByIdWithValidId_ReturnsValidOrder()
        {
            var order = new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[0]);

            sut.InsertAsync(shop.Id, order).Wait();

            var task = sut.FindByIdAsync(shop.Id, order.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(order.FullyEquals(result), Is.True);
        }

        [Test]
        public void FindByIdWithNotValidId_ReturnsNull()
        {
            var order = new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[0]);

            var task = sut.FindByIdAsync(shop.Id, order.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Null);
        }

        private IEnumerable<Order> PrepareForFindAll()
        {
            var orders = new Order[]
            {
                new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[0]),
                new Order(Guid.Empty, DateTime.Now, new Customer("", "", ""), carts[1]),
            };

            foreach (var order in orders)
                sut.InsertAsync(shop.Id, order).Wait();

            return orders;
        }

        [Test]
        public void FindAllForShop_ReturnsAllInShop()
        {
            var orders = PrepareForFindAll();

            var task = sut.FindAllForShopAsync(shop.Id, DateTime.Now.AddMinutes(-1), DateTime.Now.AddMinutes(1));
            task.Wait();
            var result = task.Result;

            Assert.That(orders.All(o => result?.Where(r => r.FullyEquals(o)).Count() == 1), Is.True);
        }
    }
}
