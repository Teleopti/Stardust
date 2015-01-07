using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Rhino.Mocks;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Controllers
{
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
			var site = new Site(" ");
			site.SetId(Guid.NewGuid());
			var team = new Team { Description = new Description(" ") };
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
			var teamId = Guid.NewGuid();
			var teamAdherenceAggregator = MockRepository.GenerateMock<ITeamAdherenceAggregator>();
			teamAdherenceAggregator.Stub(x => x.Aggregate(teamId)).Return(expected);
			
			var target = new setup()
			             {
				             TeamAdherenceAggregator = teamAdherenceAggregator
			             }
				.CreateController();

			var result = target.GetOutOfAdherence(teamId.ToString()).Data as TeamOutOfAdherence;

			result.Id.Should().Be(teamId.ToString());
			result.OutOfAdherence.Should().Be(expected);
		}

		[Test]
		public void ShouldGetOutOfAdherencerForAllTeams()
		{
			var team1 = Guid.NewGuid().ToString();
			var team2 = Guid.NewGuid().ToString();


			var getAdherence = MockRepository.GenerateMock<IGetAdherence>();
			var target = new TeamsController(getAdherence);
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
			var site = new Site(" ").WithId();
			var bu = new BusinessUnit(" ").WithId();
			site.SetBusinessUnit(bu);
			var team = new Team().WithId();
			team.Site = site;
			var teamRepository = MockRepository.GenerateMock<ITeamRepository>();
			teamRepository.Stub(x => x.Get(team.Id.GetValueOrDefault())).Return(team);
		
			var target = new setup()
			             {
				             TeamRepository = teamRepository
			             }
				.CreateController();

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

		private static Guid addNewTeamOnSite(ISite site)
		{
			return addNewTeamOnSite(site, RandomName.Make());
		}

		private static Site addNewSiteToRepository(ISiteRepository siteRepository)
		{
			var site = new Site(" ");
			site.SetId(Guid.NewGuid());
			siteRepository.Stub(x => x.Get(site.Id.Value)).Return(site);
			return site;
		}

		private class setup
		{
			public ISiteRepository SiteRepository { get; set; }
			public INumberOfAgentsInTeamReader NumberOfAgentsInTeamReader { get; set; }
			public ITeamAdherenceAggregator TeamAdherenceAggregator { get; set; }
			public ITeamRepository TeamRepository { get; set; }
			public ITeamAdherencePersister TeamAdherencePersister { get; set; }
			public ISiteAdherencePersister SiteAdherencePersister { get; set; }

			public TeamsController CreateController()
			{
				var logic = new GetAdherence(SiteRepository, NumberOfAgentsInTeamReader, TeamAdherenceAggregator,
				TeamRepository, TeamAdherencePersister, SiteAdherencePersister);
				return new TeamsController(logic);
			}
		} 
	}
}
