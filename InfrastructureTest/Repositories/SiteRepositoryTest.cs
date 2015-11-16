using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;
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

        protected override Repository<ISite> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new SiteRepository(currentUnitOfWork);
        }

        [Test]
        public void ShouldFindSiteByName()
        {
            var name = "test";
            ISite site = new Site(name);
            PersistAndRemoveFromUnitOfWork(site);

            var sites = new SiteRepository(UnitOfWork).FindSiteByDescriptionName(name).ToList();
            Assert.AreEqual(name, sites[0].Description.Name);
        }

        [Test]
        public void VerifyCreateInstance()
        {
            ISite site = new Site("test");
            Assert.AreEqual("test",site.Description.Name);
        }

		[Test]
		public void ShouldFindSiteContains()
		{
			var name = RandomName.Make();
			var site = new Site(RandomName.Make() + name + RandomName.Make());
			PersistAndRemoveFromUnitOfWork(site);

			var loaded = new SiteRepository(UnitOfWork).FindSitesContain(name, 20);

			loaded.Should().Have.SameValuesAs(site);
		}

		[Test]
		public void ShouldMissSiteContains()
		{
			var site = new Site(RandomName.Make());
			PersistAndRemoveFromUnitOfWork(site);

			var loaded = new SiteRepository(UnitOfWork).FindSitesContain(RandomName.Make(), 20);

			loaded.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyFetchMaxNumberOfItems()
		{
			const int maxHits = 2;
			var name = RandomName.Make();
			PersistAndRemoveFromUnitOfWork(new Site(name));
			PersistAndRemoveFromUnitOfWork(new Site(name));
			PersistAndRemoveFromUnitOfWork(new Site(name));

			var loaded = new SiteRepository(UnitOfWork).FindSitesContain(name, maxHits);

			loaded.Count().Should().Be.EqualTo(maxHits);
		}

		[Test]
		public void ShouldNotMakeSearchCallIfMaxItemsIsZero()
		{
			var statementsBefore = Session.SessionFactory.Statistics.PrepareStatementCount;

			new SiteRepository(UnitOfWork).FindSitesContain(RandomName.Make(), 0);

			var statementsAfter = Session.SessionFactory.Statistics.PrepareStatementCount;
			(statementsAfter - statementsBefore).Should().Be.EqualTo(0);
		}
	}
}