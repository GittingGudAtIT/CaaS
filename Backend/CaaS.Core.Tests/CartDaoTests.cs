using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DB;
using CaaS.Core.Domain;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client.Extensions.Msal;
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
    public class CartDaoTests
    {
        private IConnectionFactory connectionFactory;
        private ICartDao sut;
        private readonly Shop shop = new (Guid.Empty, "shop1", "shopkey");
        private readonly Product[] products = new[]
        {
            new Product(Guid.Empty, "prod0", 3.33M, "some words",0, "some link"),
            new Product(Guid.Empty, "prod1", 9.99M, "some words",1, "some link"),
            new Product(Guid.Empty, "prod2", 1.82M, "some words",2, "some link"),
            new Product(Guid.Empty, "prod3", 5.25M, "some words",3, "some link"),
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
            var productDao = new AdoProductDao(connectionFactory);

            shopDao.InsertAsync(shop).Wait();
            foreach (var product in products)
                productDao.InsertAsync(shop.Id, product).Wait();

            sut = new AdoCartDao(connectionFactory);
        }

        [TearDown]
        public void AfterEach()
        {
            var task = connectionFactory.CreateConnectionAsync();
            task.Wait();

            using DbConnection connection = task.Result;
            using DbCommand command = connection.CreateCommand();

            command.CommandText = "delete from cartentry";
            command.ExecuteNonQuery();
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
            var cart = new Cart(Guid.Empty, products.Select(x => new ProductAmount(x, 3)));

            sut.InsertAsync(shop.Id, cart).Wait();

            Assert.That(cart.Id, Is.Not.EqualTo(Guid.Empty));
        }


        [Test]
        public void FindById_IfNotEmpty_ReturnsValidCart()
        {
            var cart = new Cart(Guid.Empty, products.Select(x => new ProductAmount(x, 3)));

            sut.InsertAsync(shop.Id, cart).Wait();
            var task = sut.FindByIdAsync(shop.Id, cart.Id);
            task.Wait();

            var result = task.Result;

            CollectionAssert.AreEquivalent(cart, result);
        }


        [Test]
        public void Delete_ReallyDeletes()
        {
            var cart = new Cart(Guid.Empty, products.Select(x => new ProductAmount(x, 3)));

            sut.InsertAsync(shop.Id, cart).Wait();
            sut.DeleteAsync(shop.Id, cart.Id).Wait();

            var task = sut.FindByIdAsync(shop.Id, cart.Id);
            task.Wait();

            var result = task.Result;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Update_UpdatesCart()
        {
            var cart = new Cart(Guid.Empty, products.Select(x => new ProductAmount(x, 3)));
            sut.InsertAsync(shop.Id, cart).Wait();

            int i = 0;
            cart = new Cart(cart.Id, cart.Where(x => i++ % 2 == 0));
            cart.ForEach(x => x.Count = 5);
            sut.UpdateAsync(shop.Id, cart).Wait();

            var task = sut.FindByIdAsync(shop.Id, cart.Id);
            task.Wait();
            var result = task.Result;

            CollectionAssert.AreEquivalent(cart, result);
        }
    }
}
