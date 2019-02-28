using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.Helper;
using Teleopti.Ccc.TestCommon.FakeData;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	/// <summary>
	/// Testclass for BusinessUnitRepository
	/// </summary>
	[TestFixture]
    [Category("BucketB")]
    public class BusinessUnitRepositoryTest : DatabaseTest  
    {
        private BusinessUnitRepository rep;

        protected override void SetupForRepositoryTest()
        {
            rep = BusinessUnitRepository.DONT_USE_CTOR(UnitOfWork);
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
            loadedList.Remove(BusinessUnitUsedInTests.BusinessUnit);
            Assert.AreEqual(2, loadedList.Count); 
            Assert.IsTrue(loadedList[0].Equals(bu2));       
        }



        [Test]
        public void VerifyCanLoadAllTimeZones()
        {
            PersistAndRemoveFromUnitOfWork(BusinessUnitUsedInTests.BusinessUnit);

            ISkillType type = SkillTypeFactory.CreateSkillTypePhone();
            PersistAndRemoveFromUnitOfWork(type);


            Activity activity = new Activity("test");
            PersistAndRemoveFromUnitOfWork(activity);

            ISkill skill = SkillFactory.CreateSkill("NewName",type,15);
            skill.TimeZone = TimeZoneInfo.GetSystemTimeZones()[1];
            skill.Activity = activity;
            PersistAndRemoveFromUnitOfWork(skill);

            IPerson person = PersonFactory.CreatePerson("test");
            person.PermissionInformation.SetDefaultTimeZone((TimeZoneInfo.FindSystemTimeZoneById("GMT Standard Time")));
            PersistAndRemoveFromUnitOfWork(person);

            IEnumerable<TimeZoneInfo> allTimeZones = BusinessUnitRepository.DONT_USE_CTOR(UnitOfWork).LoadAllTimeZones();

            Assert.IsTrue(allTimeZones.Any(t => t.Id == person.PermissionInformation.DefaultTimeZone().Id));
            Assert.IsTrue(allTimeZones.Any(t => t.Id == skill.TimeZone.Id));
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

            BusinessUnitUsedInTests.BusinessUnit.AddSite(site1);

            PersistAndRemoveFromUnitOfWork(BusinessUnitUsedInTests.BusinessUnit);

            IBusinessUnitRepository repository = BusinessUnitRepository.DONT_USE_CTOR(UnitOfWork);
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