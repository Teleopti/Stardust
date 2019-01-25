using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon.FakeData;

namespace Teleopti.Ccc.DomainTest.Common
{
    [TestFixture]
    public class SiteTest
    {
        private ISite _target;
        protected ISite UnitWithOneTeam => _target;

	    [SetUp]
        public void Setup()
        {
            _target = SiteFactory.CreateSiteWithOneTeam();
        }

        [Test]
        public void CanCreateAndPropertiesAreSet()
        {
            Assert.IsNotNull(UnitWithOneTeam);
            Assert.IsNotNull(UnitWithOneTeam.TeamCollection);
            Assert.AreEqual(null, UnitWithOneTeam.Id);
        }

		[Test]
		public void ShouldPublishSiteNameChangedEvent()
		{
			var siteId = Guid.NewGuid();
			var target = new Site("name");
			target.SetId(siteId);

			target.SetDescription(new Description("london"));
			
			var result = target.PopAllEvents(null).OfType<SiteNameChangedEvent>().Single();
			result.SiteId.Should().Be(siteId);
			result.Name.Should().Be("london");
		}

		[Test]
        public void TeamsPropertyShouldBeLocked()
        {
            ICollection<ITeam> temp = UnitWithOneTeam.TeamCollection;
            Assert.Throws<NotSupportedException>(() => temp.Add(TeamFactory.CreateSimpleTeam()));
        }

		[Test]
        public void VerifyNameCanBeSetAndGet()
        {
            const string setValue = "Set Name";

            UnitWithOneTeam.SetDescription(new Description(setValue));
            string resultValue = UnitWithOneTeam.Description.Name;

            Assert.AreEqual(setValue, resultValue);
        }
		
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
		
        [Test]
        public void NullTeamsAreNotAllowed()
        {
            Assert.Throws<ArgumentNullException>(() => UnitWithOneTeam.AddTeam(null));
        }

        [Test]
        public void NullTeamsAreNotAllowedToRemove()
        {
            Assert.Throws<ArgumentNullException>(() => UnitWithOneTeam.RemoveTeam(null));
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