using CaaS.Core.Dal.Ado;
using CaaS.Core.Dal.Common;
using CaaS.Core.Dal.Interface;
using CaaS.Core.DB;
using CaaS.Core.DBLayer.Domain.TestExtensions;
using CaaS.Core.Domain;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace CaaS.Core.Tests
{
    [TestFixture]
    public class ProductDaoTests
    {
        private IConnectionFactory connectionFactory;
        private IProductDao sut;
        private readonly Shop shop = new(Guid.Empty, "shop1", "shopkey");
        private readonly Shop shop2 = new(Guid.Empty, "shop2", "shop2key");
        private readonly Product apple = new(Guid.Empty, "apple.png", 3.3M, "picture of an apple", 0, "https://apples/435/donwload");
        private readonly Product banana = new(Guid.Empty, "banana.png", 3.3M, "picture of a banana", 1, "https://banana/435/donwload");

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
            shopDao.InsertAsync(shop2).Wait();

            sut = new AdoProductDao(connectionFactory);
        }

        [TearDown]
        public void AfterEach()
        {
            var task = connectionFactory.CreateConnectionAsync();
            task.Wait();

            using DbConnection connection = task.Result;
            using DbCommand command = connection.CreateCommand();

            command.CommandText = "delete from product";
            command.ExecuteNonQuery();
        }


        [OneTimeTearDown]
        public void Cleanup()
        {
            var manager = new DbManager(connectionFactory);
            manager.DropAsync().Wait();
        }

        [Test]
        public void Insert_UpdatesProductId()
        {
            apple.Id = Guid.Empty;

            sut.InsertAsync(shop.Id, apple).Wait();

            Assert.That(apple.Id, Is.Not.EqualTo(Guid.Empty));
        }

        [Test]
        public void FindByIdWithExistingId_ReturnsValidProduct()
        {
            sut.InsertAsync(shop.Id, apple).Wait();

            var task = sut.FindByIdAsync(shop.Id, apple.Id);
            task.Wait();

            var result = task.Result;

            Assert.That(result, Is.Not.Null);
            Assert.That(apple.FullyEquals(result), Is.True);
        }

        [Test]
        public void FindByMultipleIds_ReturnsExpectedCollection()
        {
            sut.InsertAsync(shop.Id, apple).Wait();
            sut.InsertAsync(shop.Id, banana).Wait();

            var task = sut.FindByIdAsync(shop.Id, new Guid[] { apple.Id, banana.Id });
            task.Wait();
            var result = task.Result;
            Assert.Multiple(() =>
            {
                Assert.That(result.Count(), Is.EqualTo(2));
                Assert.That(result.Where(r => r.FullyEquals(apple)).Count(), Is.EqualTo(1));
                Assert.That(result.Where(r => r.FullyEquals(banana)).Count(), Is.EqualTo(1));
            });
        }

        [Test]
        public void FindByIdWithInvalidId_ReturnsNull()
        {
            var task = sut.FindByIdAsync(shop.Id, apple.Id);
            task.Wait();

            var result = task.Result;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Update_UpdatesPriceAndReturnsTrue()
        {
            sut.InsertAsync(shop.Id, apple).Wait();

            var banana = new Product(apple.Id, "banana", 5.5M, "picture of banane", 0, "bananlink");
            sut.UpdateAsync(banana).Wait();

            var selTask = sut.FindByIdAsync(shop.Id, apple.Id);
            selTask.Wait();
            var selResult = selTask.Result;

            Assert.That(selResult, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(selResult.Id, Is.EqualTo(apple.Id));
                Assert.That(selResult.Name, Is.EqualTo(apple.Name));
                Assert.That(selResult.Description, Is.EqualTo(apple.Description));
                Assert.That(selResult.Price, Is.EqualTo(banana.Price));
                Assert.That(selResult.DownloadLink, Is.EqualTo(apple.DownloadLink));
            });
        }

        [Test]
        public void FindAll_GetsAllOfSelectedShop()
        {
            sut.DeleteAsync(apple.Id).Wait();

            var productsShop1 = new Product[]
            {
                new Product(Guid.Empty, "p1", 3.5M, "",0, ""),
                new Product(Guid.Empty, "p2", 9.1M, "",1, ""),
                new Product(Guid.Empty, "p3", 2.6M, "",2, ""),
                new Product(Guid.Empty, "p4", 1.1M, "",3, ""),
            };

            var productsShop2 = new Product[]
            {
                new Product(Guid.Empty, "p1", 9.5M, "",0, ""),
                new Product(Guid.Empty, "p2", 1.5M, "",1, ""),
            };

            foreach (var product in productsShop1)
                sut.InsertAsync(shop.Id, product).Wait();

            foreach(var product in productsShop2)
                sut.InsertAsync(shop2.Id, product).Wait();

            var taskS1 = sut.FindAllAsync(shop.Id);
            taskS1.Wait();
            var resultS1 = taskS1.Result;

            var taskS2 = sut.FindAllAsync(shop2.Id);
            taskS2.Wait();
            var resultS2 = taskS2.Result;

            Assert.Multiple(() =>
            {
                Assert.That(productsShop1
                    .Where(p => resultS1.Where(r => r.FullyEquals(p)).Count() == 1)
                    .Count(), 
                    Is.EqualTo(productsShop1.Length)
                );
                Assert.That(productsShop2
                        .Where(p => resultS2.Where(r => r.FullyEquals(p)).Count() == 1)
                        .Count(), 
                    Is.EqualTo(productsShop2.Length)
                );
            });
        }

        [Test]
        public void Delete_RemovesFromTable()
        {
            sut.DeleteAsync(apple.Id).Wait();

            var task = sut.FindByIdAsync(shop.Id, apple.Id);
            task.Wait();
            var result = task.Result;

            Assert.That(result, Is.Null);
        }

        [Test]
        public void Search_ReturnsSetOfEqualSoundingProducts()
        {
            var products = new Product[]
            {
                new Product(Guid.Empty, "kescher", 3.3M, "",0, ""),
                new Product(Guid.Empty, "nearly kescher", 3.2M, "",1, ""),
                new Product(Guid.Empty, "something else", 3.9M, "",2, ""),
                new Product(Guid.Empty, "fresher", 7.3M, "",3, ""),
            };

            foreach (var product in products)
                sut.InsertAsync(shop.Id, product).Wait();

            var task = sut.SearchAsync(shop.Id, "kascher");
            task.Wait();
            var result = task.Result;

            Assert.Multiple(() =>
            {
                CollectionAssert.IsNotEmpty(result);
                if (result.Any())
                    Assert.That(result.First(), Is.EqualTo(products[0]));
            });
        }

        [Test]
        public void FindShopForProduct_ReturnsValidShop()
        {
            var product = new Product(Guid.Empty, "my product", 3.3M, "this is a description", 0, "");
            sut.InsertAsync(shop.Id, product).Wait();

            var task = sut.FindShopFromProductAsync(product.Id);
            task.Wait();

            var result = task.Result;

            Assert.That(shop.FullyEquals(result), Is.True);
        }
    }
}
