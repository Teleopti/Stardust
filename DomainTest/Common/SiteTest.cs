using System;
using System.Collections.Generic;
using NUnit.Framework;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.Common
{
    /// <summary>
    /// Class for unit tests
    /// </summary>
    [TestFixture]
    public class SiteTest
    {
        private ISite _target;
        
        /// <summary>
        /// Gets the unit with one team.
        /// </summary>
        /// <value>The simple unit with one team.</value>
        protected ISite UnitWithOneTeam
        {
            get { return _target; }
        }

        /// <summary>
        /// Runs once per test.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _target = SiteFactory.CreateSiteWithOneTeam();
        }

        /// <summary>
        /// Determines whether this instance can be created and properties are set.
        /// </summary>
        [Test]
        public void CanCreateAndPropertiesAreSet()
        {
            Assert.IsNotNull(UnitWithOneTeam);
            Assert.IsNotNull(UnitWithOneTeam.TeamCollection);
            Assert.AreEqual(null, UnitWithOneTeam.Id);
        }

        /// <summary>
        /// Teams property should be locked.
        /// </summary>
        [Test]
        [ExpectedException(typeof(NotSupportedException))]
        public void TeamsPropertyShouldBeLocked()
        {
            ICollection<ITeam> temp = UnitWithOneTeam.TeamCollection;
            temp.Add(TeamFactory.CreateSimpleTeam());
        }

        /// <summary>
        /// Verifies the name can be set and get.
        /// </summary>
        [Test]
        public void VerifyNameCanBeSetAndGet()
        {
            const string setValue = "Set Name";

            UnitWithOneTeam.Description = new Description(setValue);
            string resultValue = UnitWithOneTeam.Description.Name;

            Assert.AreEqual(setValue, resultValue);
        }
        
        /// <summary>
        /// Verifies a layer can be added.
        /// </summary>
        [Test]
        public void VerifyAgentsCanBeAdded()
        {
            int countBefore = UnitWithOneTeam.TeamCollection.Count;
            Team testTeam1 = TeamFactory.CreateSimpleTeam();
            Team testTeam2 = TeamFactory.CreateSimpleTeam();

            UnitWithOneTeam.AddTeam(testTeam1);
            UnitWithOneTeam.AddTeam(testTeam2);

            Assert.Contains(testTeam1, UnitWithOneTeam.TeamCollection);
            Assert.AreEqual(UnitWithOneTeam, testTeam1.Site);
            Assert.AreEqual(UnitWithOneTeam, testTeam2.Site);

            Assert.Contains(testTeam2, UnitWithOneTeam.TeamCollection);
            Assert.AreEqual(countBefore + 2, UnitWithOneTeam.TeamCollection.Count);
        }

        /// <summary>
        /// Duplicate team instances should be ignored when added to list.
        /// </summary>
        [Test]
        public void DoNotDuplicateTeamInstancesWhenAddedToList()
        {
            int countBefore = UnitWithOneTeam.TeamCollection.Count;
            int lastPosBeforeAdded = countBefore - 1;
            Team testTeam = TeamFactory.CreateSimpleTeam();

            UnitWithOneTeam.AddTeam(testTeam);
            UnitWithOneTeam.AddTeam(testTeam);
            UnitWithOneTeam.AddTeam(testTeam);

            Assert.AreSame(testTeam, UnitWithOneTeam.TeamCollection[lastPosBeforeAdded + 1]);
            Assert.AreEqual(countBefore + 1, UnitWithOneTeam.TeamCollection.Count);
        }

        /// <summary>
        /// Null teams are not allowed.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTeamsAreNotAllowed()
        {
            UnitWithOneTeam.AddTeam(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullTeamsAreNotAllowedToRemove()
        {
            UnitWithOneTeam.RemoveTeam(null);
        }

        [Test]
        public void VerifyRemoveTeamFromSite()
        {
            var site = UnitWithOneTeam;
            int teamCount = site.TeamCollection.Count;

            site.RemoveTeam(site.TeamCollection[0]);
            Assert.AreEqual(teamCount - 1, site.TeamCollection.Count);
        }
        
		[Test]
		public void ShouldHaveANullableMaxSeatsProperty()
		{
			Assert.That(_target.MaxSeats, Is.Null);
			_target.MaxSeats = 55;
			Assert.That(_target.MaxSeats,Is.EqualTo(55));
		}

		[Test]
		public void ShouldReturnSortedTeamList()
		{
			var teamA = TeamFactory.CreateSimpleTeam("A");
			var teamB = TeamFactory.CreateSimpleTeam("B");
			var teamC = TeamFactory.CreateSimpleTeam("C");

			_target = SiteFactory.CreateSimpleSite("site");

			_target.AddTeam(teamC);
			_target.AddTeam(teamA);
			_target.AddTeam(teamB);

			var teams = _target.SortedTeamCollection;

			Assert.AreEqual(teamA, teams[0]);
			Assert.AreEqual(teamB, teams[1]);
			Assert.AreEqual(teamC, teams[2]);
		}
    }
}