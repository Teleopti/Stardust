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
        public void VerifyCombineAvailableDataInstances()
        {
            IAvailableData inputData1 = new AvailableData();
            IPerson person1 = PersonFactory.CreatePerson("Person1");
            inputData1.AddAvailablePerson(person1);
            ISite site1 = new Site("Site");
            inputData1.AddAvailableSite(site1);
            ITeam team1 = TeamFactory.CreateSimpleTeam("Team1");
            site1.AddTeam(team1);
            inputData1.AddAvailableTeam(team1);
            IBusinessUnit unit1 = new BusinessUnit("Unit1");
            inputData1.AddAvailableBusinessUnit(unit1);
            inputData1.AvailableDataRange = AvailableDataRangeOption.MyOwn;

            IAvailableData returnData = AvailableData.CombineAvailableData(new List<IAvailableData> { inputData1} );
            Assert.AreEqual(returnData.AvailableBusinessUnits, inputData1.AvailableBusinessUnits);
            Assert.AreEqual(returnData.AvailableSites, inputData1.AvailableSites);
            Assert.AreEqual(returnData.AvailableTeams, inputData1.AvailableTeams);
            Assert.AreEqual(returnData.AvailablePersons, inputData1.AvailablePersons);

            IAvailableData inputData2 = new AvailableData();
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            inputData2.AddAvailablePerson(person2);
            ISite site2 = new Site("Site2");
            inputData2.AddAvailableSite(site2);
            ITeam team2 = TeamFactory.CreateSimpleTeam("Team2");
            site2.AddTeam(team2);
            inputData2.AddAvailableTeam(team2);
            IBusinessUnit unit2 = new BusinessUnit("Unit2");
            inputData2.AddAvailableBusinessUnit(unit2);
            inputData2.AvailableDataRange = AvailableDataRangeOption.MySite;

            returnData = AvailableData.CombineAvailableData(new List<IAvailableData> {inputData1, inputData2 } );
            Assert.AreEqual(inputData2.AvailableBusinessUnits.Count + inputData1.AvailableBusinessUnits.Count, returnData.AvailableBusinessUnits.Count);
            Assert.AreEqual(inputData2.AvailableSites.Count + inputData1.AvailableSites.Count, returnData.AvailableSites.Count);
            Assert.AreEqual(inputData2.AvailableTeams.Count + inputData1.AvailableTeams.Count, returnData.AvailableTeams.Count);
            Assert.AreEqual(inputData2.AvailablePersons.Count + inputData1.AvailablePersons.Count, returnData.AvailablePersons.Count);
            Assert.AreEqual(AvailableDataRangeOption.MySite, returnData.AvailableDataRange);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void VerifyCombineAvailableDataInstancesWithNullInput()
        {
            AvailableData.CombineAvailableData(null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyFindByApplicationRole()
        {
            IAvailableData availableData1 = new AvailableData();
            ((IAggregateRoot)availableData1).SetId(new Guid());
            IApplicationRole applicationRole1 = new ApplicationRole();
            availableData1.ApplicationRole = applicationRole1;
            
            IAvailableData availableData2 = new AvailableData();
            ((IAggregateRoot)availableData2).SetId(new Guid());
            IApplicationRole applicationRole2 = new ApplicationRole();
            availableData2.ApplicationRole = applicationRole2;

            IList<IAvailableData> availableDataList = new List<IAvailableData>{ availableData1, availableData2 };

            IAvailableData result = AvailableData.FindByApplicationRole(availableDataList, applicationRole2);
            Assert.AreEqual(result, availableData1);

            result = AvailableData.FindByApplicationRole(availableDataList, new ApplicationRole());
            Assert.IsNull(result);

        }

        /// <summary>
        /// Verifies the convert to available data entry collection method.
        /// </summary>
        /// <remarks>
        /// Define an AvailableData with some business units, sites, teams and agents.
        /// Run method and verify.
        /// </remarks>
        [Test]
        public void VerifyConvertToAvailableDataEntryCollection()
        {
            _target = AvailableDataFactory.CreateAvailableDataWithTwoBusinessUnitsSitesTeamsPersons();
        
            IList<IAvailableDataEntry> result = _target.ConvertToPermittedDataEntryCollection();

            Assert.AreEqual(8, result.Count);
            Assert.AreEqual("Unit1 Business Unit", result[0].AuthorizationDescription);
            Assert.AreEqual("Unit2 Business Unit", result[1].AuthorizationDescription);
            Assert.AreEqual("Site1 Site", result[2].AuthorizationDescription);
            Assert.AreEqual("Site2 Site", result[3].AuthorizationDescription);
            Assert.AreEqual("Team1 Team", result[4].AuthorizationDescription);
            Assert.AreEqual("Team2 Team", result[5].AuthorizationDescription);
            Assert.AreEqual("Person1 Person1 Agent", result[6].AuthorizationDescription);
            Assert.AreEqual("Person2 Person2 Agent", result[7].AuthorizationDescription);
        }

        [Test]
        public void VerifyConvertToAvailablePersonCollectionWithBusinessUnit()
        {
            IList<ITeam> teams;
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out teams);

            //ISkill skill = SkillFactory.CreateSkill("Skill");
            DateOnly startDate = new DateOnly(2001, 01, 01);
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[0]);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[1]);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[2]);

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            person1.AddPersonPeriod(personPeriod1);
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            person2.AddPersonPeriod(personPeriod2);
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            person3.AddPersonPeriod(personPeriod3);
            
            _target.AddAvailableBusinessUnit(bu);

            IList<IPerson> result =
               _target.ConvertToPermittedPersonCollection(new List<IPerson> { person1, person2, person3 }, new DateOnlyPeriod(2000, 01, 01,2002, 01, 01));

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToAvailablePersonCollectionWithSites()
        {
            IList<ITeam> teams;
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out teams);

            //ISkill skill = SkillFactory.CreateSkill("Skill");
            DateOnly startDate = new DateOnly(2001, 01, 01);
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[0]);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[1]);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[2]);

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            person1.AddPersonPeriod(personPeriod1);
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            person2.AddPersonPeriod(personPeriod2);
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            person3.AddPersonPeriod(personPeriod3);

            _target.AddAvailableSite(bu.SiteCollection[0]);
            _target.AddAvailableSite(bu.SiteCollection[1]);

            IList<IPerson> result =
               _target.ConvertToPermittedPersonCollection(new List<IPerson> { person1, person2, person3 }, new DateOnlyPeriod(2000, 01, 01,2002, 01, 01));

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToAvailablePersonCollectionWithPersons()
        {
            IList<ITeam> teams;
            BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out teams);

            //ISkill skill = SkillFactory.CreateSkill("Skill");
            DateOnly startDate = new DateOnly(2001, 01, 01);
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[0]);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[1]);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[2]);

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            person1.AddPersonPeriod(personPeriod1);
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            person2.AddPersonPeriod(personPeriod2);
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            person3.AddPersonPeriod(personPeriod3);

            _target.AddAvailablePerson(person1);
            _target.AddAvailablePerson(person2);
            _target.AddAvailablePerson(person3);

            IList<IPerson> result =
               _target.ConvertToPermittedPersonCollection(new List<IPerson> { person1, person2, person3 }, new DateOnlyPeriod(2000, 01, 01,2002, 01, 01));

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToAvailablePersonCollectionWithTeams()
        {
            IList<ITeam> teams;
            BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out teams);

            //ISkill skill = SkillFactory.CreateSkill("Skill");
            DateOnly startDate = new DateOnly(2001, 01, 01);
            IPersonPeriod personPeriod1 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[0]);
            IPersonPeriod personPeriod2 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[1]);
            IPersonPeriod personPeriod3 = PersonPeriodFactory.CreatePersonPeriod(startDate, teams[2]);

            IPerson person1 = PersonFactory.CreatePerson("Person1");
            person1.AddPersonPeriod(personPeriod1);
            IPerson person2 = PersonFactory.CreatePerson("Person2");
            person2.AddPersonPeriod(personPeriod2);
            IPerson person3 = PersonFactory.CreatePerson("Person3");
            person3.AddPersonPeriod(personPeriod3);

            _target.AddAvailableTeam(teams[0]);
            _target.AddAvailableTeam(teams[1]);
            _target.AddAvailableTeam(teams[2]);

            IList<IPerson> result =
               _target.ConvertToPermittedPersonCollection(new List<IPerson> { person1, person2, person3 }, new DateOnlyPeriod(2000, 01, 01, 2002, 01, 01));

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToPermittedSiteCollectionWithBusinessUnit()
        {
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
           _target.AddAvailableBusinessUnit(bu);

            IList<ISite> result = _target.ConvertToPermittedSiteCollection();

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void VerifyConvertToPermittedSiteCollectionWithSites()
        {
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();

            _target.AddAvailableSite(bu.SiteCollection[0]);
            _target.AddAvailableSite(bu.SiteCollection[1]);

            IList<ISite> result = _target.ConvertToPermittedSiteCollection();

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void VerifyConvertToPermittedTeamCollectionWithBusinessUnit()
        {
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            _target.AddAvailableBusinessUnit(bu);

            IList<ITeam> result = ((AvailableData)_target).ConvertToPermittedTeamCollection();

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToPermittedTeamCollectionWithSites()
        {
            IBusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();

            _target.AddAvailableSite(bu.SiteCollection[0]);
            _target.AddAvailableSite(bu.SiteCollection[1]);

            IList<ITeam> result = ((AvailableData)_target).ConvertToPermittedTeamCollection();

            Assert.AreEqual(3, result.Count);
        }

        [Test]
        public void VerifyConvertToPermittedTeamCollectionWithTeams()
        {
            IList<ITeam> teams;
            BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams(out teams);

            _target.AddAvailableTeam(teams[0]);
            _target.AddAvailableTeam(teams[1]);
            _target.AddAvailableTeam(teams[2]);

            IList<ITeam> result = ((AvailableData)_target).ConvertToPermittedTeamCollection();

            Assert.AreEqual(3, result.Count);
        }

	    [Test]
		public void VerifyApplicationRole()
		{
			// Declare variable to hold property set method
			IApplicationRole setValue = ApplicationRoleFactory.CreateRole("TestRole", "TestRole");

			// Test set method
			_target.ApplicationRole = setValue;

			// Declare return variable to hold property get method
			IApplicationRole getValue;

			// Test get method
			getValue = _target.ApplicationRole;
			
			// Perform Assert Tests
			Assert.AreSame(setValue, getValue);
		}

		[Test]
		public void VerifyAvailableBusinessUnits()
		{
			// Test set method
			_target.AddAvailableBusinessUnit(new BusinessUnit("TestUnit"));

			// Declare return variable to hold property get method
			ICollection<IBusinessUnit> getValue;

			// Test get method
			getValue = _target.AvailableBusinessUnits;
			
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
            ICollection<IBusinessUnit> getValue;

            // Test get method
            getValue = _target.AvailableBusinessUnits;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

		[Test]
		public void VerifyAvailableSites()
		{
			// Test set method
			_target.AddAvailableSite(new Site("TestSite"));

			// Declare return variable to hold property get method
			ICollection<ISite> getValue;

			// Test get method
			getValue = _target.AvailableSites;
			
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
			ICollection<ITeam> getValue;

			// Test get method
			getValue = _target.AvailableTeams;
			
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
            ICollection<ITeam> getValue;

            // Test get method
            getValue = _target.AvailableTeams;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

	    [Test]
		public void VerifyAvailableAgents()
		{
			// Test set method
			_target.AddAvailablePerson(new Person());

			// Declare return variable to hold property get method
			ICollection<IPerson> getValue;

			// Test get method
			getValue = _target.AvailablePersons;
			
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
            ICollection<IPerson> getValue;

            // Test get method
            getValue = _target.AvailablePersons;

            // Perform Assert Tests
            Assert.AreEqual(1, getValue.Count);
        }

        [Test]
        public void VerifyAvailableDataRange()
        {
            AvailableDataRangeOption setValue = AvailableDataRangeOption.MyOwn;

            _target.AvailableDataRange = setValue;

            AvailableDataRangeOption getValue;

            getValue = _target.AvailableDataRange;

            Assert.AreEqual(setValue, getValue);
        }

        /// <summary>
        /// Verifies the delete available person.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
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

        /// <summary>
        /// Verifies the delete available team.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
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

        /// <summary>
        /// Verifies the delete available site.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
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

        /// <summary>
        /// Verifies the delete available business unit.
        /// </summary>
        /// <remarks>
        /// Created by:SanjayaI
        /// Created date: 5/2/2008
        /// </remarks>
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
