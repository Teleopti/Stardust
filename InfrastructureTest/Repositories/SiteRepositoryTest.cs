using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for Site repository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class SiteRepositoryTest : RepositoryTest<ISite>
    {
        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
        }

        /// <summary>
        /// Creates an aggregate using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override ISite CreateAggregateWithCorrectBusinessUnit()
        {
            return SiteFactory.CreateSimpleSite("for test");
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(ISite loadedAggregateFromDatabase)
        {
            Assert.AreEqual(BusinessUnitFactory.BusinessUnitUsedInTest, loadedAggregateFromDatabase.BusinessUnit);
            Assert.AreEqual("for test", loadedAggregateFromDatabase.Description.Name);
        }

        protected override Repository<ISite> TestRepository(IUnitOfWork unitOfWork)
        {
            return new SiteRepository(unitOfWork);
        }

        [Test]
        public void ShouldFindSiteByName()
        {
            var name = "test";
            ISite site = new SiteRepository(UnitOfWork).CreateInstance(name);
            PersistAndRemoveFromUnitOfWork(site);

            var sites = new SiteRepository(UnitOfWork).FindSiteByDescriptionName(name).ToList();
            Assert.AreEqual(name, sites[0].Description.Name);
        }

        [Test]
        public void VerifyCreateInstance()
        {
            ISite site = new SiteRepository(UnitOfWork).CreateInstance("test");
            Assert.AreEqual("test",site.Description.Name);
        }
    }
}