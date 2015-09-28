using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
	[IoCTest]
	[TestFixture]
	public class TeamsControllerTest2 : ISetup
	{
		public FakeSiteRepository SiteRepository;
		public FakeNumberOfAgentsInTeamReader NumberOfAgentsInTeam;
		public TeamsController Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeNumberOfAgentsInTeamReader>().For<INumberOfAgentsInTeamReader>();
		}

		[Test]
		public void ShouldGetMultipleTeamsForSites()
		{
			var site1 = new Site("site1").WithId();
			var team1 = new Team { Description = new Description("team1") }.WithId();
			site1.AddTeam(team1);
			SiteRepository.Has(site1);

			var site2 = new Site("site2").WithId();
			var team2 = new Team { Description = new Description("team2") }.WithId();
			site2.AddTeam(team2);
			SiteRepository.Has(site2);

			var result = Target.ForSites(new[] { site1.Id.ToString(), site2.Id.ToString() }).Data as IEnumerable<TeamViewModel>;
			result.Single(x => x.Id == team1.Id.ToString()).Name.Should().Be("team1");
			result.Single(x => x.Id == team1.Id.ToString()).SiteId.Should().Be(site1.Id.ToString());

			result.Single(x => x.Id == team2.Id.ToString()).Name.Should().Be("team2");
			result.Single(x => x.Id == team2.Id.ToString()).SiteId.Should().Be(site2.Id.ToString());
		}

		[Test]
		public void ShouldGetNumberOfAgentsForMultipleTeamsOnSites()
		{
			var site = new Site("site1").WithId();
			var team = new Team { Description = new Description("team1") }.WithId();
			site.AddTeam(team);
			SiteRepository.Has(site);
			NumberOfAgentsInTeam.Has(team, 3);

			var result = Target.ForSites(new[] { site.Id.ToString() }).Data as IEnumerable<TeamViewModel>;
			result.Single(x => x.Id == team.Id.ToString()).NumberOfAgents.Should().Be(3);
		}
	}



	[TestFixture]
	public class TeamsControllerTest
	{
		
		[Test]
		public void ShouldGetTeamsForSite()
		{
			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var site = addNewSiteToRepository(siteRepository);
			var team = addNewTeamOnSite(site, "team1");

			var target = new setup()
			             {
				           SiteRepository = siteRepository,
			             }
						 .CreateController();
			
			var result = target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().Name.Should().Be("team1");
			result.Single().Id.Should().Be(team.ToString());
		}

		

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			const int expected = 2;

			var siteRepository = MockRepository.GenerateMock<ISiteRepository>();
			var numberOfAgentsQuery = MockRepository.GenerateMock<INumberOfAgentsInTeamReader>();
			var site = new Site("s");
			site.SetId(Guid.NewGuid());
			var team = new Team { Description = new Description("t") };
			team.SetId(Guid.NewGuid());
			site.AddTeam(team);
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);
			numberOfAgentsQuery.Stub(x => x.FetchNumberOfAgents(new[] { team })).Return(new Dictionary<Guid, int>() { { team.Id.Value, expected } });

			var target = new setup()
			             {
							 SiteRepository = siteRepository,
							 NumberOfAgentsInTeamReader = numberOfAgentsQuery
						 }
						 .CreateController();

			var result = target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().NumberOfAgents.Should().Be.EqualTo(expected);
		}


		[Test]
		public void ShouldGetOutOfAdherenceForTeam()
		{
			const int expected = 1;
			var teamId = Guid.NewGuid().ToString();
			var getAdherence = MockRepository.GenerateMock<IGetTeamAdherence>();
			var teamOutOfAdherence = new TeamOutOfAdherence {Id = teamId, OutOfAdherence = 1};
			getAdherence.Stub(x => x.GetOutOfAdherence(teamId)).Return(teamOutOfAdherence);

			var target = new TeamsController(getAdherence, null);
			var result = target.GetOutOfAdherence(teamId).Data as TeamOutOfAdherence;

			result.Id.Should().Be(teamId);
			result.OutOfAdherence.Should().Be(expected);
		}

		[Test]
		public void ShouldGetOutOfAdherencerForAllTeams()
		{
			var team1 = Guid.NewGuid().ToString();
			var team2 = Guid.NewGuid().ToString();


			var getAdherence = MockRepository.GenerateMock<IGetTeamAdherence>();
			var target = new TeamsController(getAdherence,null);
			var siteId = Guid.NewGuid();

			getAdherence.Expect(g => g.GetOutOfAdherenceForTeamsOnSite(siteId.ToString()))
				.Return(new List<TeamOutOfAdherence>()
				        {
					        new TeamOutOfAdherence() { Id = team1, OutOfAdherence = 1},
					        new TeamOutOfAdherence() { Id = team2, OutOfAdherence = 2},
				        });

			var result = target.GetOutOfAdherenceForTeamsOnSite(siteId.ToString()).Data as IEnumerable<TeamOutOfAdherence>;

			result.Single(x => x.Id == team1).OutOfAdherence.Should().Be(1);
			result.Single(x => x.Id == team2).OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldGetBusinessUnitIdFromTeamId()
		{
			var site = new Site("s").WithId();
			var bu = new BusinessUnit("bu").WithId();
			site.SetBusinessUnit(bu);
			var team = new Team().WithId();
			team.Site = site;
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(team.Id.GetValueOrDefault())).Return(team);
		
			var target = new setup {GetBusinessUnitId = new GetBusinessUnitId(teamRepository)}.CreateController();

			var result = target.GetBusinessUnitId(team.Id.ToString());
			result.Data.Should().Be(bu.Id.GetValueOrDefault());
		}

		private static Guid addNewTeamOnSite(ISite site, string teamName)
		{
			var teamId = Guid.NewGuid();
			var team = new Team { Description = new Description(teamName) };
			site.AddTeam(team);
			team.SetId(teamId);
			return teamId;
		}

		private static Site addNewSiteToRepository(ISiteRepository siteRepository)
		{
			var site = new Site("s");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);
			return site;
		}

		private class setup
		{
			public ISiteRepository SiteRepository { get; set; }
			public INumberOfAgentsInTeamReader NumberOfAgentsInTeamReader { get; set; }
			public ITeamAdherenceAggregator TeamAdherenceAggregator { get; set; }
			public IGetBusinessUnitId GetBusinessUnitId { get; set; }

			public TeamsController CreateController()
			{
				var logic = new GetTeamAdherence(SiteRepository, NumberOfAgentsInTeamReader,TeamAdherenceAggregator,null);
				return new TeamsController(logic, GetBusinessUnitId);
			}
		} 
	}
}
