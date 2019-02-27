using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure;
using Teleopti.Ccc.Domain.Kpi;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.Repositories;
using Teleopti.Ccc.InfrastructureTest.UnitOfWork;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.TestData;

namespace Teleopti.Ccc.InfrastructureTest.Repositories
{
	/// <summary>
	/// Tests for TeamRepository
	/// </summary>
	[TestFixture]
	[Category("BucketB")]
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

			scorecard = new Scorecard { Name = "test" };
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
			Assert.AreEqual(org.Scorecard, loadedAggregateFromDatabase.Scorecard);
		}

		/// <summary>
		/// Determines whether this instance can be created.
		/// </summary>
		[Test]
		public void CanCreate()
		{
			ITeam team = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(team);
			ICollection<ITeam> teams = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindAllTeamByDescription();
			Assert.AreEqual(1, teams.Count);
			teams.First().Site.GetType()
				.Should().Be.EqualTo(typeof(Site));
		}

		[Test]
		public void ShouldFindTeamByName()
		{
			ITeam team = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(team);

			IList<ITeam> teams = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamByDescriptionName(teamName).ToList();
			Assert.AreEqual(teamName, teams[0].Description.Name);
		}

		[Test]
		public void ShouldFindTeamByIds()
		{
			ITeam team = CreateAggregateWithCorrectBusinessUnit();
			ITeam team2 = CreateAggregateWithCorrectBusinessUnit();
			PersistAndRemoveFromUnitOfWork(team);
			PersistAndRemoveFromUnitOfWork(team2);

			var teams = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeams(new List<Guid> { team.Id.Value, team2.Id.Value });
			Assert.That(teams.Count, Is.EqualTo(2));
			Assert.That(LazyLoadingManager.IsInitialized(teams.First().Site), Is.True);
		}


		[Test]
		public void ShouldFindTeamContain()
		{
			var name = RandomName.Make();
			var team = new Team { Site = teamSite }.WithDescription(new Description(RandomName.Make() + name + RandomName.Make()));
			PersistAndRemoveFromUnitOfWork(team);

			var loaded = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamsContain(name, 20);

			loaded.Should().Have.SameValuesAs(team);
		}

		[Test]
		public void ShouldMissTeamContain()
		{
			var team = new Team { Site = teamSite }.WithDescription(new Description(RandomName.Make()));
			PersistAndRemoveFromUnitOfWork(team);

			var loaded = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamsContain(RandomName.Make(), 20);

			loaded.Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyFetchMaxNumberOfItems()
		{
			const int maxHits = 2;
			var name = RandomName.Make();
			PersistAndRemoveFromUnitOfWork(new Team { Site = teamSite }.WithDescription(new Description(name)));
			PersistAndRemoveFromUnitOfWork(new Team { Site = teamSite }.WithDescription(new Description(name)));
			PersistAndRemoveFromUnitOfWork(new Team { Site = teamSite }.WithDescription(new Description(name)));

			var loaded = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamsContain(name, maxHits);

			loaded.Count().Should().Be.EqualTo(maxHits);
		}

		[Test]
		public void ShouldNotMakeSearchCallIfMaxItemsIsZero()
		{
			using (Session.SessionFactory.WithStats())
			{
				var statementsBefore = Session.SessionFactory.Statistics.PrepareStatementCount;

				TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamsContain(RandomName.Make(), 0);

				var statementsAfter = Session.SessionFactory.Statistics.PrepareStatementCount;
				(statementsAfter - statementsBefore).Should().Be.EqualTo(0);
			}
		}

		[Test]
		public void ShouldLoadBySite()
		{
			var site = new Site("Paris");
			var anotherSite = new Site("Rome");
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(anotherSite);
			PersistAndRemoveFromUnitOfWork(new Team { Site = site }
				.WithDescription(new Description("A")));
			var teams = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeamsForSite(site.Id.Value);

			teams.Select(x => x.Description.Name).Single().Should().Be("A");
		}

		[Test]
		public void ShouldDistinctTeamsWhenCallFindTeams()
		{
			var site = SiteFactory.CreateSimpleSite("Site");
			var team = TeamFactory.CreateSimpleTeam("Team 1");
			team.Site = site;
			PersistAndRemoveFromUnitOfWork(site);
			PersistAndRemoveFromUnitOfWork(team);

			var teamIds =new List<Guid>();

			for (var i = 0; i < 500; i++) {
				teamIds.Add(team.Id.Value);
			}
			var teams = TeamRepository.DONT_USE_CTOR(UnitOfWork).FindTeams(teamIds);

			teams.Count.Should().Be.EqualTo(1);
		}


		protected override Repository<ITeam> TestRepository(ICurrentUnitOfWork currentUnitOfWork)
		{
			return TeamRepository.DONT_USE_CTOR(currentUnitOfWork);
		}
	}
}