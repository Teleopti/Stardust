using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using NUnit.Framework;
using Teleopti.Ccc.Domain.Security.AuthorizationEntities;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Security.AuthorizationEntities
{
	[TestFixture]
	public class AvailableDataTest
	{

		private IAvailableData _target;

	    [SetUp]
		public void TestInit()
		{
			_target = new AvailableData();
		}
		
		[TearDown]
		public void TestDispose()
		{
			_target = null;
		}

	    [Test]
		public void VerifyConstructor()
		{
			Assert.IsNotNull(_target);
            Assert.IsNotNull(_target.AvailableBusinessUnits);
            Assert.IsNotNull(_target.AvailablePersons);
            Assert.IsNotNull(_target.AvailableSites);
            Assert.IsNotNull(_target.AvailableTeams);
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyFindByApplicationRole()
        {
            IAvailableData availableData1 = new AvailableData();
            availableData1.SetId(new Guid());
            IApplicationRole applicationRole1 = new ApplicationRole();
            availableData1.ApplicationRole = applicationRole1;
            
            IAvailableData availableData2 = new AvailableData();
            availableData2.SetId(new Guid());
            IApplicationRole applicationRole2 = new ApplicationRole();
            availableData2.ApplicationRole = applicationRole2;

            IList<IAvailableData> availableDataList = new List<IAvailableData>{ availableData1, availableData2 };

            IAvailableData result = AvailableData.FindByApplicationRole(availableDataList, applicationRole2);
            Assert.AreEqual(result, availableData1);

            result = AvailableData.FindByApplicationRole(availableDataList, new ApplicationRole());
            Assert.IsNull(result);
        }

	    [Test]
		public void VerifyApplicationRole()
		{
			// Declare variable to hold property set method
			IApplicationRole setValue = ApplicationRoleFactory.CreateRole("TestRole", "TestRole");

			// Test set method
			_target.ApplicationRole = setValue;

			// Declare return variable to hold property get method

		    // Test get method
			IApplicationRole getValue = _target.ApplicationRole;
			
			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifyAvailableBusinessUnits()
		{
			// Test set method
			_target.AddAvailableBusinessUnit(new BusinessUnit("TestUnit"));

			// Declare return variable to hold property get method

			// Test get method
			ICollection<IBusinessUnit> getValue = _target.AvailableBusinessUnits;
			
			// Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableBusinessUnitsWithSameBusinessUnit()
        {
            // Test set method
            IBusinessUnit unit = new BusinessUnit("TestUnit");

            _target.AddAvailableBusinessUnit(unit);
            _target.AddAvailableBusinessUnit(unit);

            // Declare return variable to hold property get method

	        // Test get method
            ICollection<IBusinessUnit> getValue = _target.AvailableBusinessUnits;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

		[Test]
		public void VerifyAvailableSites()
		{
			// Test set method
			_target.AddAvailableSite(new Site("TestSite"));

			// Declare return variable to hold property get method

			// Test get method
			ICollection<ISite> getValue = _target.AvailableSites;
			
			// Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableSitesWithSameSite()
        {
            ISite site = new Site("TestSite");
            
            _target.AddAvailableSite(site);
            _target.AddAvailableSite(site);

            // Declare return variable to hold property get method

            // Test get method
            ICollection<ISite> getValue = _target.AvailableSites;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

		[Test]
		public void VerifyAvailableTeams()
		{

			// Test set method
			_target.AddAvailableTeam(new Team());

			// Declare return variable to hold property get method

			// Test get method
			ICollection<ITeam> getValue = _target.AvailableTeams;
			
			// Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailableTeamsWithSameTeam()
        {
            ITeam team = new Team();
            
            _target.AddAvailableTeam(team);
            _target.AddAvailableTeam(team);

            // Declare return variable to hold property get method

	        // Test get method
            ICollection<ITeam> getValue = _target.AvailableTeams;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

	    [Test]
		public void VerifyAvailableAgents()
		{
			// Test set method
			_target.AddAvailablePerson(new Person());

			// Declare return variable to hold property get method

		    // Test get method
			ICollection<IPerson> getValue = _target.AvailablePersons;
			
			// Perform Assert Tests
			Assert.AreEqual(1, getValue.Count);
		}

        [Test]
        public void VerifyAddAvailablePersonsWithSamePerson()
        {
            IPerson person = new Person();

            _target.AddAvailablePerson(person);
            _target.AddAvailablePerson(person);

            // Declare return variable to hold property get method

	        // Test get method
            ICollection<IPerson> getValue = _target.AvailablePersons;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

        [Test]
        public void VerifyAvailableDataRange()
        {
            var setValue = AvailableDataRangeOption.MyOwn;

            _target.AvailableDataRange = setValue;

	        AvailableDataRangeOption getValue = _target.AvailableDataRange;

            Assert.AreEqual(setValue, getValue);
        }

        [Test]
        public void VerifyDeleteAvailablePerson()
        {
            Person person = new Person();
           
            _target.AddAvailablePerson(person);
            Assert.AreEqual(1, _target.AvailablePersons.Count);
            Assert.IsTrue(_target.AvailablePersons.Contains(person));


            _target.DeleteAvailablePerson(person);
            Assert.AreEqual(0, _target.AvailablePersons.Count);
            Assert.IsFalse(_target.AvailablePersons.Contains(person));
        }

        [Test]
        public void VerifyDeleteAvailableTeam()
        {
            Team team = new Team();

            _target.AddAvailableTeam(team);
            Assert.AreEqual(1,_target.AvailableTeams.Count);
            Assert.IsTrue(_target.AvailableTeams.Contains(team));

            _target.DeleteAvailableTeam(team);
            Assert.AreEqual(0,_target.AvailableTeams.Count);
            Assert.IsFalse(_target.AvailableTeams.Contains(team));
        }

        [Test]
        public void VerifyDeleteAvailableSite()
        {
            Site site = new Site("site");

            _target.AddAvailableSite(site);
            Assert.AreEqual(1,_target.AvailableSites.Count);
            Assert.IsTrue(_target.AvailableSites.Contains(site));

            _target.DeleteAvailableSite(site);
            Assert.AreEqual(0,_target.AvailableSites.Count);
            Assert.IsFalse(_target.AvailableSites.Contains(site));
        }

        [Test]
        public void VerifyDeleteAvailableBusinessUnit()
        {
            BusinessUnit businessUnit = new BusinessUnit("businessUnit");
            
            _target.AddAvailableBusinessUnit(businessUnit);
            Assert.AreEqual(1,_target.AvailableBusinessUnits.Count);
            Assert.IsTrue(_target.AvailableBusinessUnits.Contains(businessUnit));

            _target.DeleteAvailableBusinessUnit(businessUnit);
            Assert.AreEqual(0,_target.AvailableBusinessUnits.Count);
            Assert.IsFalse(_target.AvailableBusinessUnits.Contains(businessUnit));
        }

        [Test]
        public void VerifyDeleted()
        {
            bool isDeleted = ((AvailableData) _target).IsDeleted;
            Assert.IsFalse(isDeleted);
            ((AvailableData) _target).SetDeleted();
            isDeleted = ((AvailableData)_target).IsDeleted;
            Assert.IsTrue(isDeleted);
        }
	}

}
