using System;
using System.Collections.Generic;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Time;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using System.Linq;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Testclass for BusinessUnitRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class BusinessUnitRepositoryTest : DatabaseTest  
    {
        private BusinessUnitRepository rep;

        protected override void SetupForRepositoryTest()
        {
            rep = new BusinessUnitRepository(UnitOfWork);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void CanCreate()
        {
            new BusinessUnitRepository(UnitOfWorkFactory.Current).Should().Not.Be.Null();
        }

        [Test]
        public void VerifyLoadAllBusinessUnitSortedByName()
        {
            IBusinessUnit bu1 = new BusinessUnit("_BusinessUnitB");
            IBusinessUnit bu2 = new BusinessUnit("_BusinessUnitA");
            PersistAndRemoveFromUnitOfWork(bu1);
            PersistAndRemoveFromUnitOfWork(bu2);
            //load
            IList<IBusinessUnit> loadedList = rep.LoadAllBusinessUnitSortedByName();
            loadedList.Remove(BusinessUnitFactory.BusinessUnitUsedInTest);
            Assert.AreEqual(2, loadedList.Count); 
            Assert.IsTrue(loadedList[0].Equals(bu2));       
        }



        [Test]
        public void VerifyCanLoadAllTimeZones()
        {
            PersistAndRemoveFromUnitOfWork(BusinessUnitFactory.BusinessUnitUsedInTest);

            ISkillType type = SkillTypeFactory.CreateSkillType();
            PersistAndRemoveFromUnitOfWork(type);

            GroupingActivity ga = GroupingActivityFactory.CreateSimpleGroupingActivity("test");
            PersistAndRemoveFromUnitOfWork(ga);

            Activity activity = ActivityFactory.CreateActivity("test");
            activity.GroupingActivity = ga;
            PersistAndRemoveFromUnitOfWork(activity);

            ISkill skill = SkillFactory.CreateSkill("NewName",type,15);
            skill.TimeZone = new CccTimeZoneInfo(TimeZoneInfo.GetSystemTimeZones()[1]);
            skill.Activity = activity;
            PersistAndRemoveFromUnitOfWork(skill);

            IPerson person = PersonFactory.CreatePerson("test");
            person.PermissionInformation.SetDefaultTimeZone(new CccTimeZoneInfo(TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")));
            PersistAndRemoveFromUnitOfWork(person);

            IEnumerable<TimeZoneInfo> allTimeZones = new BusinessUnitRepository(UnitOfWork).LoadAllTimeZones();

            Assert.IsTrue(allTimeZones.Any(t => t.Id == person.PermissionInformation.DefaultTimeZone().Id));
            Assert.IsTrue(allTimeZones.Any(t => t.Id == skill.TimeZone.Id));
        }

        [Test]
        [Ignore("Why does not this work??? See together with Roger /tamasb")]
        public void VerifyLoadHierarchyInformation()
        {
            //setup
            ISite site1 = SiteFactory.CreateSimpleSite("SITE1");
            PersistAndRemoveFromUnitOfWork(site1);
            ISite site2 = SiteFactory.CreateSimpleSite("SITE2");
            PersistAndRemoveFromUnitOfWork(site2);

            ITeam site1team1 = TeamFactory.CreateSimpleTeam("SITE1TEAM1");
            site1team1.Site = site1;
            PersistAndRemoveFromUnitOfWork(site1team1);
            ITeam site1team2 = TeamFactory.CreateSimpleTeam("SITE1TEAM2");
            site1team2.Site = site1;
            PersistAndRemoveFromUnitOfWork(site1team2);
            ITeam site2team1 = TeamFactory.CreateSimpleTeam("SITE2TEAM1");
            site2team1.Site = site2;
            PersistAndRemoveFromUnitOfWork(site2team1);

            IBusinessUnit bu = BusinessUnitFactory.CreateSimpleBusinessUnit("BU1");
            bu.AddSite(site1);
            bu.AddSite(site2);
            PersistAndRemoveFromUnitOfWork(bu);

            //load
            IBusinessUnitRepository repository = new BusinessUnitRepository(UnitOfWork);
            //IList<IBusinessUnit> loadedList = rep.LoadAllBusinessUnitSortedByName();
            //IBusinessUnit loadedBusinessUnit = loadedList[0];
            IBusinessUnit loadedBusinessUnit = rep.Load(bu.Id.Value);

            loadedBusinessUnit = repository.LoadHierarchyInformation(loadedBusinessUnit);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedBusinessUnit.SiteCollection));
            Assert.AreEqual(2, loadedBusinessUnit.SiteCollection.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedBusinessUnit.SiteCollection[0].TeamCollection));
            Assert.AreEqual(2, loadedBusinessUnit.SiteCollection[0].TeamCollection.Count);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedBusinessUnit.SiteCollection[1].TeamCollection));
            Assert.AreEqual(1, loadedBusinessUnit.SiteCollection[1].TeamCollection.Count);
        }

        [Test]
        public void ShouldBeInitializedAfterLoadedHierarchyInformation()
        {
            ISite site1 = SiteFactory.CreateSiteWithOneTeam("Site1");
            PersistAndRemoveFromUnitOfWork(site1);

            ITeam site1team1 = TeamFactory.CreateSimpleTeam("SITE1TEAM1");
            site1team1.Site = site1;
            PersistAndRemoveFromUnitOfWork(site1team1);

            ITeam site1team2 = TeamFactory.CreateSimpleTeam("SITE1TEAM2");
            site1team2.Site = site1;
            PersistAndRemoveFromUnitOfWork(site1team2);

            BusinessUnitFactory.BusinessUnitUsedInTest.AddSite(site1);

            PersistAndRemoveFromUnitOfWork(BusinessUnitFactory.BusinessUnitUsedInTest);

            IBusinessUnitRepository repository = new BusinessUnitRepository(UnitOfWork);
            IList<IBusinessUnit> loadedList = rep.LoadAllBusinessUnitSortedByName();
            IBusinessUnit loadedBusinessUnit = loadedList[0];

            loadedBusinessUnit = repository.LoadHierarchyInformation(loadedBusinessUnit);
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedBusinessUnit.SiteCollection));
            Assert.IsTrue(LazyLoadingManager.IsInitialized(loadedBusinessUnit.SiteCollection[0].TeamCollection));
            Assert.AreEqual(1, loadedBusinessUnit.SiteCollection.Count);
            Assert.AreEqual(2, loadedBusinessUnit.SiteCollection[0].TeamCollection.Count);
        }
    }
}