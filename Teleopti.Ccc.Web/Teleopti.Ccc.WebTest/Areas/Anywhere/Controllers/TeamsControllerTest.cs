using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
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
	public class TeamsControllerTest : ISetup
	{
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeNumberOfAgentsInTeamReader NumberOfAgentsInTeam;
		public FakeTeamAdherenceAggregator TeamAdherenceAggregator;
		public FakeTeamOutOfAdherenceReadModelReader TeamOutOfAdherenceReadModelReader;
		public TeamsController Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeNumberOfAgentsInTeamReader>().For<INumberOfAgentsInTeamReader>();
			system.UseTestDouble<FakeTeamAdherenceAggregator>().For<ITeamAdherenceAggregator>();
			system.UseTestDouble<FakeTeamOutOfAdherenceReadModelReader>().For<ITeamOutOfAdherenceReadModelReader>();
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();
		}
		
		[Test]
		public void ShouldGetTeamsForSite()
		{
			var teamId = Guid.NewGuid();
			var site = new Site("site").WithId();
			site.AddTeam(new Team {Description = new Description("team1")}
						.WithId(teamId));
			SiteRepository.Has(site);
			
			var result = Target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().Name.Should().Be("team1");
			result.Single().Id.Should().Be(teamId.ToString());
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			var team = new Team {Description = new Description("t")}.WithId();
			var site = new Site("s").WithId();
			site.AddTeam(team);
			SiteRepository.Has(site);
			NumberOfAgentsInTeam.Has(team, 2);
			
			var result = Target.ForSite(site.Id.Value.ToString()).Data as IEnumerable<TeamViewModel>;

			result.Single().NumberOfAgents.Should().Be.EqualTo(2);
		}
		
		[Test]
		public void ShouldGetBusinessUnitIdFromTeamId()
		{
			var businessUnit = new BusinessUnit("bu").WithId();
			var site = new Site("s").WithId();
			var team = new Team().WithId();
			site.AddTeam(team);
			businessUnit.AddSite(site);
			site.SetBusinessUnit(businessUnit);
			
			TeamRepository.Has(team);
			
			Target.GetBusinessUnitId(team.Id.ToString()).Data.Should().Be(businessUnit.Id.GetValueOrDefault());
		}
		
		[Test]
		public void ShouldGetOutOfAdherenceForTeam()
		{
			const int expected = 1;
			var teamId = Guid.NewGuid();
			TeamAdherenceAggregator.Has(teamId, 1);
			
			var result = Target.GetOutOfAdherence(teamId.ToString()).Data as TeamOutOfAdherence;

			result.Id.Should().Be(teamId.ToString());
			result.OutOfAdherence.Should().Be(expected);
		}

		[Test, Ignore("Incorrect")]
		public void ShouldGetOutOfAdherencerForAllTeams()
		{
			var team1 = Guid.NewGuid();
			var team2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();

			TeamOutOfAdherenceReadModelReader.Has(siteId, team1, 1);
			TeamOutOfAdherenceReadModelReader.Has(siteId, team2, 2);
			
			var result = Target.GetOutOfAdherenceForTeamsOnSite(siteId.ToString()).Data as IEnumerable<TeamOutOfAdherence>;

			result.Single(x => x.Id == team1.ToString()).OutOfAdherence.Should().Be(1);
			result.Single(x => x.Id == team2.ToString()).OutOfAdherence.Should().Be(2);
		}

		[Test]
		public void ShouldGetMultipleTeamsForSites()
		{
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var site1 = new Site("site1").WithId();
			site1.AddTeam(
					new Team {Description = new Description("team1")}
						.WithId(teamId1));
			var site2 = new Site("site2").WithId();
			site2.AddTeam(
					new Team {Description = new Description("team2")}
						.WithId(teamId2));
			SiteRepository.Has(site1);
			SiteRepository.Has(site2);

			var result = Target.ForSites(new[] { site1.Id.ToString(), site2.Id.ToString() }).Data as IEnumerable<TeamViewModel>;

			result.Single(x => x.Id == teamId1.ToString()).Name.Should().Be("team1");
			result.Single(x => x.Id == teamId1.ToString()).SiteId.Should().Be(site1.Id.ToString());
			result.Single(x => x.Id == teamId2.ToString()).Name.Should().Be("team2");
			result.Single(x => x.Id == teamId2.ToString()).SiteId.Should().Be(site2.Id.ToString());
		}

		[Test]
		public void ShouldGetNumberOfAgentsForMultipleTeamsOnSites()
		{
			var team = new Team { Description = new Description("team1") }.WithId();
			var site = new Site("site1").WithId();
			site.AddTeam(team);
			
			SiteRepository.Has(site);
			NumberOfAgentsInTeam.Has(team, 3);

			var result = Target.ForSites(new[] { site.Id.ToString() }).Data as IEnumerable<TeamViewModel>;

			result.Single(x => x.Id == team.Id.ToString()).NumberOfAgents.Should().Be(3);
		}
	}
}
