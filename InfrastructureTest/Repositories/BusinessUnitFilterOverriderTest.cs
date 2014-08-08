using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    [TestFixture]
    [Category("LongRunning")]
    public class BusinessUnitFilterOverriderTest : DatabaseTest
    {
        [Test]
        public void ShouldSaveEntityInAnotherBusinessUnit()
        {
            var bu = new BusinessUnit("BU 1");
            PersistAndRemoveFromUnitOfWork(bu);
            var buId = bu.Id.Value;
            var target = new BusinessUnitFilterOverrider(CurrentUnitOfWork.Make(), new CurrentIdentity());
            
            var site = new Site("1");
            site.SetBusinessUnit(bu);
            using (target.OverrideWith(buId))
            {    
                PersistAndRemoveFromUnitOfWork(site);
                var sites = new SiteRepository(CurrentUnitOfWork.Make()).LoadAll();
                sites.Single().Should().Be(site);
            }
        }

        [Test]
        public void ShouldChangeBackToOriginalBusinessUnit()
        {
            var bu1 = new BusinessUnit("BU 1");
            PersistAndRemoveFromUnitOfWork(bu1);

            var target = new BusinessUnitFilterOverrider(CurrentUnitOfWork.Make(), new CurrentIdentity());

            var site = new Site("1");
            site.SetBusinessUnit(bu1);
            using (target.OverrideWith(bu1.Id.Value))
            {
                PersistAndRemoveFromUnitOfWork(site);
            }

            var sites = new SiteRepository(CurrentUnitOfWork.Make()).LoadAll();
            sites.Count.Should().Be(0);
        }
    }
}
