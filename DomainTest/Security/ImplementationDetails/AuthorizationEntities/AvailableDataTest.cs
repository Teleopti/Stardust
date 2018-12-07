using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Security.ImplementationDetails.AuthorizationEntities
{
	[TestFixture]
	public class AvailableDataTest
	{
	    [Test]
		public void VerifyConstructor()
		{
			var target = new AvailableData();
			Assert.IsNotNull(target);
            Assert.IsNotNull(target.AvailableBusinessUnits);
            Assert.IsNotNull(target.AvailableSites);
            Assert.IsNotNull(target.AvailableTeams);
		}

	    [Test]
		public void VerifyApplicationRole()
		{
			var target = new AvailableData();
			var setValue = ApplicationRoleFactory.CreateRole("TestRole", "TestRole");

			target.ApplicationRole = setValue;

			IApplicationRole getValue = target.ApplicationRole;
			
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifyAvailableBusinessUnits()
		{
			var target = new AvailableData();
			target.AddAvailableBusinessUnit(new BusinessUnit("TestUnit"));

			ICollection<IBusinessUnit> getValue = target.AvailableBusinessUnits;
			
            Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableBusinessUnitsWithSameBusinessUnit()
        {
			var target = new AvailableData(); 
			var unit = new BusinessUnit("TestUnit");

            target.AddAvailableBusinessUnit(unit);
            target.AddAvailableBusinessUnit(unit);

            ICollection<IBusinessUnit> getValue = target.AvailableBusinessUnits;

            Assert.AreEqual(1, getValue.Count);
        }

		[Test]
		public void VerifyAvailableSites()
		{
			var target = new AvailableData(); 
			target.AddAvailableSite(new Site("TestSite"));

			ICollection<ISite> getValue = target.AvailableSites;
			
			Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableSitesWithSameSite()
        {
			var target = new AvailableData(); 
			ISite site = new Site("TestSite");
            
            target.AddAvailableSite(site);
            target.AddAvailableSite(site);

			ICollection<ISite> getValue = target.AvailableSites;

            Assert.AreEqual(1, getValue.Count);
        }

		[Test]
		public void VerifyAvailableTeams()
		{
			var target = new AvailableData();
			target.AddAvailableTeam(new Team());

			ICollection<ITeam> getValue = target.AvailableTeams;
			
			Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableTeamsWithSameTeam()
		{
			var target = new AvailableData();
            ITeam team = new Team();
            
            target.AddAvailableTeam(team);
            target.AddAvailableTeam(team);

			ICollection<ITeam> getValue = target.AvailableTeams;

            Assert.AreEqual(1, getValue.Count);
        }

        [Test]
        public void VerifyAvailableDataRange()
        {
			var target = new AvailableData(); 
			const AvailableDataRangeOption setValue = AvailableDataRangeOption.MyOwn;

            target.AvailableDataRange = setValue;

	        AvailableDataRangeOption getValue = target.AvailableDataRange;

            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDeleteAvailableTeam()
        {
			var target = new AvailableData(); 
			var team = new Team();

            target.AddAvailableTeam(team);
            Assert.AreEqual(1,target.AvailableTeams.Count);
            Assert.IsTrue(target.AvailableTeams.Contains(team));

            target.DeleteAvailableTeam(team);
            Assert.AreEqual(0,target.AvailableTeams.Count);
            Assert.IsFalse(target.AvailableTeams.Contains(team));
        }

        [Test]
        public void VerifyDeleteAvailableSite()
		{
			var target = new AvailableData();
            var site = new Site("site");

            target.AddAvailableSite(site);
            Assert.AreEqual(1,target.AvailableSites.Count);
            Assert.IsTrue(target.AvailableSites.Contains(site));

            target.DeleteAvailableSite(site);
            Assert.AreEqual(0,target.AvailableSites.Count);
            Assert.IsFalse(target.AvailableSites.Contains(site));
        }

        [Test]
        public void VerifyDeleteAvailableBusinessUnit()
		{
			var target = new AvailableData();
            var businessUnit = new BusinessUnit("businessUnit");
            
            target.AddAvailableBusinessUnit(businessUnit);
            Assert.AreEqual(1,target.AvailableBusinessUnits.Count);
            Assert.IsTrue(target.AvailableBusinessUnits.Contains(businessUnit));

            target.DeleteAvailableBusinessUnit(businessUnit);
            Assert.AreEqual(0,target.AvailableBusinessUnits.Count);
            Assert.IsFalse(target.AvailableBusinessUnits.Contains(businessUnit));
        }

        [Test]
        public void VerifyDeleted()
        {
			var availableData = new AvailableData();
	        bool isDeleted = availableData.IsDeleted;
            Assert.IsFalse(isDeleted);
            availableData.SetDeleted();
            isDeleted = availableData.IsDeleted;
            Assert.IsTrue(isDeleted);
        }
	}
}
