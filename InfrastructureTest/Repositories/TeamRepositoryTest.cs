using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.Infrastructure.UnitOfWork;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
    /// <summary>
    /// Tests for TeamRepository
    /// </summary>
    [TestFixture]
    [Category("LongRunning")]
    public class TeamRepositoryTest : RepositoryTest<ITeam>
    {
        private ISite teamSite;
        private IScorecard scorecard;
        private const string teamName = "Morsmors";

        /// <summary>
        /// Runs every test. Implemented by repository's concrete implementation.
        /// </summary>
        protected override void ConcreteSetup()
        {
            teamSite = SiteFactory.CreateSimpleSite("hejhej");
            PersistAndRemoveFromUnitOfWork(teamSite);

            scorecard = new Scorecard {Name = "test"};
            PersistAndRemoveFromUnitOfWork(scorecard);
        }


        /// <summary>
        /// Creates an aggreagte using the Bu of logged in user.
        /// Should be a "full detailed" aggregate
        /// </summary>
        /// <returns></returns>
        protected override ITeam CreateAggregateWithCorrectBusinessUnit()
        {
            ITeam team = TeamFactory.CreateSimpleTeam(teamName);
            teamSite.AddTeam(team);
            team.Scorecard = scorecard;
            return team;
        }

        /// <summary>
        /// Verifies the aggregate graph properties.
        /// </summary>
        /// <param name="loadedAggregateFromDatabase">The loaded aggregate from database.</param>
        protected override void VerifyAggregateGraphProperties(ITeam loadedAggregateFromDatabase)
        {
            ITeam org = CreateAggregateWithCorrectBusinessUnit();
            Assert.IsNotNull(loadedAggregateFromDatabase.Id);
            Assert.AreEqual(org.Description.Name, loadedAggregateFromDatabase.Description.Name);
            Assert.IsNotNull(loadedAggregateFromDatabase.Site);
            Assert.IsNotNull(loadedAggregateFromDatabase.Scorecard);
            Assert.AreEqual(org.Scorecard,loadedAggregateFromDatabase.Scorecard);
        }

        /// <summary>
        /// Determines whether this instance can be created.
        /// </summary>
        [Test]
        public void CanCreate()
        {
            ITeam team = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(team);
            ICollection<ITeam> teams = new TeamRepository(UnitOfWork).FindAllTeamByDescription();
            Assert.AreEqual(1,teams.Count);
            teams.First().Site.GetType()
                .Should().Be.EqualTo(typeof(Site));
        }

        [Test]
        public void ShouldFindTeamByName()
        {
            ITeam team = CreateAggregateWithCorrectBusinessUnit();
            PersistAndRemoveFromUnitOfWork(team);

            IList<ITeam> teams = new TeamRepository(UnitOfWork).FindTeamByDescriptionName(teamName).ToList();
            Assert.AreEqual(teamName, teams[0].Description.Name);
        }

		[Test]
		public void ShouldFindTeamByIds()
		{
			ITeam team = CreateAggregateWithCorrectBusinessUnit();
			ITeam team2 = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(team2);

			var teams = new TeamRepository(UnitOfWork).FindTeams(new List<Guid> { team.Id.Value, team2.Id.Value });
			Assert.That(teams.Count, Is.EqualTo(2));
			Assert.That(LazyLoadingManager.IsInitialized(teams.First().Site), Is.True);
		}

        protected override Repository<ITeam> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
        {
            return new TeamRepository(currentUnitOfWork);
        }
    }
}