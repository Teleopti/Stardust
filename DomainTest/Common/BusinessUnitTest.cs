using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Tests for BusinessUnit Class
    /// </summary>
    [TestFixture]
    public class BusinessUnitTest
    {
        private BusinessUnit _target;
        
        /// <summary>
        /// Setups the tests.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit("myName");
        }

        /// <summary>
        /// Test to make new BusinessUnit and checks the default Values
        /// </summary>
        [Test]
        public void CanCreateAndLoadDefaults()
        {
            Assert.AreEqual("myName", _target.Description.Name);
            Assert.IsNull(_target.UpdatedBy);
            Assert.IsNull(_target.UpdatedOn);
            Assert.IsNull(_target.Id);
            Assert.IsNull(_target.Version);
        }

        /// <summary>
        /// Verifies the doesnt belong to business unit.
        /// </summary>
        [Test]
        public void VerifyDoesNotBelongToBusinessUnit()
        {
            Assert.IsFalse(_target is IBelongsToBusinessUnit);
        }

        /// <summary>
        /// Make sure the constructor does not take null as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetNameToNull()
        {
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit(null);
        }

        /// <summary>
        /// Make sure the constructor does not take empty string as argument
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void CannotSetEmptyName()
        {
            String myName = String.Empty;
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit(myName);
        }

        /// <summary>
        /// Can change name if not empty or null
        /// </summary>
        [Test]
        public void CanChangeName()
        {
            String myName = "name";
            String myNewName = "newName";
            _target = BusinessUnitFactory.CreateSimpleBusinessUnit(myName);
            _target.Description = new Description(myNewName);
            Assert.AreEqual(_target.Description.Name, myNewName);
        }

        /// <summary>
        /// Test protectedConstructorWorks
        /// </summary>
        [Test]
        public void TestProtectedConstructorForNHibernate()
        {
            Assert.IsTrue(ReflectionHelper.HasDefaultConstructor(_target.GetType()));
        }

        /// <summary>
        /// Verifies that agents can be added.
        /// </summary>
        [Test]
        public void VerifyUnitsCanBeAdded()
        {
            int countBefore = _target.SiteCollection.Count;
            Site testUnit1 = SiteFactory.CreateSimpleSite();
            Site testUnit2 = SiteFactory.CreateSimpleSite();

            _target.AddSite(testUnit1);
            _target.AddSite(testUnit2);

            Assert.Contains(testUnit1, _target.SiteCollection);
            Assert.Contains(testUnit2, _target.SiteCollection);
            Assert.AreEqual(countBefore + 2, _target.SiteCollection.Count);
        }

        /// <summary>
        /// Duplicate layer instances should be ignored when added to list.
        /// </summary>
        [Test]
        public void DoNotDuplicateUnitInstancesWhenAddedToList()
        {
            int countBefore = _target.SiteCollection.Count;
            int lastPosBeforeAdded = countBefore - 1;
            Site testUnit = SiteFactory.CreateSimpleSite();

            _target.AddSite(testUnit);
            _target.AddSite(testUnit);
            _target.AddSite(testUnit);

            Assert.AreSame(testUnit, _target.SiteCollection[lastPosBeforeAdded + 1]);
            Assert.AreEqual(countBefore + 1, _target.SiteCollection.Count);
        }

        /// <summary>
        /// Null units are not allowed.
        /// </summary>
        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void NullUnitsAreNotAllowed()
        {
            _target.AddSite(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullSitesAreNotAllowedToRemove()
        {
            _target.RemoveSite(null);
        }

        /// <summary>
        /// Verifies the team collection property.
        /// </summary>
        [Test]
        public void VerifyTeamCollectionProperty()
        {
            Team team2 = TeamFactory.CreateSimpleTeam("Team 2");
            Team team3 = TeamFactory.CreateSimpleTeam("Team 3");
            Team team4 = TeamFactory.CreateSimpleTeam("Team 4");
            Site site1 = SiteFactory.CreateSiteWithOneTeam("Team 1");
            Site site2 = SiteFactory.CreateSimpleSite();
            site2.AddTeam(team2);
            site2.AddTeam(team3);
            site2.AddTeam(team4);

            _target.AddSite(site1);
            _target.AddSite(site2);

            ReadOnlyCollection<ITeam> resultList = _target.TeamCollection();

            Assert.AreEqual(4, resultList.Count);
            Assert.AreSame(team4, resultList[resultList.Count - 1]);
        }

        /// <summary>
        /// Verifies the site can be found from team.
        /// </summary>
        [Test]
        public void VerifySiteCanBeFoundFromTeam()
        {
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            Team team2 = TeamFactory.CreateSimpleTeam("Team 2");
            bu.SiteCollection[1].AddTeam(team2);
            Assert.IsTrue(bu.FindTeamSite(team2).Equals(bu.SiteCollection[1]));
        }

        /// <summary>
        /// Verifies the find team site returns null if site not found.
        /// </summary>
        [Test]
        public void VerifyFindTeamSiteReturnsNullIfSiteNotFound()
        {
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            Team team2 = TeamFactory.CreateSimpleTeam("Team 2");
            bu.SiteCollection[1].AddTeam(team2);
            Team team3 = TeamFactory.CreateSimpleTeam("Team 3");
            Assert.IsNull(bu.FindTeamSite(team3));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyPersonsInHierarchy()
        {
            var dateTime = new DateOnlyPeriod(2000, 1, 1, 2002, 1, 1);
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            Person person = new Person();
            Contract contract = new Contract("dummy");
            PartTimePercentage partTimePercentage = new PartTimePercentage("dummy");
            ContractSchedule contractSchedule = new ContractSchedule("dummy");
            IPersonContract personContract = new PersonContract(contract, partTimePercentage,contractSchedule);

            IPersonPeriod per = new PersonPeriod(new DateOnly(2000, 1, 1), personContract, bu.SiteCollection[0].TeamCollection[0]);
            person.AddPersonPeriod(per);

            ICollection<IPerson> candidates = new List<IPerson>();
            candidates.Add(person);
            ReadOnlyCollection<IPerson> lst = bu.PersonsInHierarchy(candidates, dateTime);

            Assert.IsNotNull(lst);
        }

        [Test]
        public void VerifyCanUseNameProperties()
        {
            _target.Name = "Test BU";
            _target.ShortName = "TBU";

            Assert.AreEqual("Test BU",_target.Name);
            Assert.AreEqual("TBU",_target.ShortName);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic"), Test]
        public void VerifyRemoveSiteFromBusinessUnit()
        {
            BusinessUnit bu = BusinessUnitFactory.CreateBusinessUnitWithSitesAndTeams();
            int siteCount = bu.SiteCollection.Count;

            bu.RemoveSite(bu.SiteCollection[0]);
            Assert.AreEqual(siteCount - 1, bu.SiteCollection.Count);
        }
    }
}