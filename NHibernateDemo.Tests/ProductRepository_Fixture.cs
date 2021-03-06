using NHibernate;
using NHibernate.Cfg;
using NHibernate.Tool.hbm2ddl;
using NHibernateDemo.Domain;
using NHibernateDemo.Repositories;
using NUnit.Framework;
using System.Collections.Generic;

namespace NHibernateDemo.Tests
{
    [TestFixture]
    public class ProductRepository_Fixture
    {
        private ISessionFactory _sessionFactory;
        private Configuration _configuration;

        private readonly Product[] _products = new[]
        {
            new Product{Name = "Melon", Category = "Fruits"},
            new Product{Name = "Pear", Category = "Fruits"},
            new Product{Name= "Milk", Category = "Beverages" },
            new Product{Name= "Coca Cola", Category = "Beverages" },
            new Product{ Name = "Pepsi Cola", Category = "Beverages"}
        };

        private void CreateInitialData()
        {
            using (ISession session = _sessionFactory.OpenSession())
            {
                using (ITransaction transaction = session.BeginTransaction())
                {
                    foreach (var product in _products)
                        session.Save(product);

                    transaction.Commit();
                }
            }
        }

        [OneTimeSetUp]
        public void TestFixtureSetUp()
        {
            _configuration = new Configuration();
            _configuration.Configure();
            _configuration.AddAssembly(typeof(Product).Assembly);
            _sessionFactory = _configuration.BuildSessionFactory();
        }

        [SetUp]
        public void SetupContext()
        {
            new SchemaExport(_configuration).Execute(false, true, false);
            CreateInitialData();
        }

        [Test]
        public void Can_add_new_product()
        {
            var product = new Product { Name = "Apple", Category = "Fruits" };
            IProductRepository repository = new ProductRepository();
            repository.Add(product);

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                // Test that the product was successfully inserted.
                Assert.IsNotNull(fromDb);
                Assert.AreNotSame(product, fromDb);
                Assert.AreEqual(product.Name, fromDb.Name);
                Assert.AreEqual(product.Category, fromDb.Category);
            }
        }

        [Test]
        public void Can_update_existing_product()
        {
            var product = _products[0];
            product.Name = "Yellow Pear";
            IProductRepository repository = new ProductRepository();
            repository.Update(product);

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                Assert.AreEqual(product.Name, fromDb.Name);
            }
        }

        [Test]
        public void Can_remove_existing_product()
        {
            var product = _products[0];
            IProductRepository repository = new ProductRepository();
            repository.Remove(product);

            using (ISession session = _sessionFactory.OpenSession())
            {
                var fromDb = session.Get<Product>(product.Id);
                Assert.IsNull(fromDb);
            }
        }

        [Test]
        public void Can_get_existing_product_by_id()
        {
            var product = _products[0];

            IProductRepository repository = new ProductRepository();

            var fromDb = repository.GetById(product.Id);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(product, fromDb);
            Assert.AreEqual(product.Name, fromDb.Name);
        }

        [Test]
        public void Can_get_product_by_name()
        {
            IProductRepository repository = new ProductRepository();
            var fromDb = repository.GetByName(_products[1].Name);

            Assert.IsNotNull(fromDb);
            Assert.AreNotSame(_products[1], fromDb);
            Assert.AreEqual(_products[1].Id, fromDb.Id);
        }

        [Test]
        public void Can_get_existing_products_by_category()
        {
            IProductRepository repository = new ProductRepository();
            ICollection<Product> fromDb = repository.GetByCategory("Fruits");

            Assert.AreEqual(2, fromDb.Count);
            Assert.IsTrue(IsInCollection(_products[0], fromDb));
            Assert.IsTrue(IsInCollection(_products[1], fromDb));
        }

        private bool? IsInCollection(Product product, ICollection<Product> fromDb)
        {
            foreach (var item in fromDb)
            {
                if (item.Id == product.Id)
                    return true;
            }
            return false;
        }
    }
}