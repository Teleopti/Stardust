using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.TestCommon.TestData;
using Teleopti.Ccc.Web.Areas.Anywhere.Controllers;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Ccc.WebTest.TestHelper;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[IoCTest]
	[TestFixture]
	public class TeamsControllerTest : ISetup
	{
		public FakeSiteRepository SiteRepository;
		public FakeTeamRepository TeamRepository;
		public FakeNumberOfAgentsInTeamReader NumberOfAgentsInTeam;
		public FakeTeamInAlarmReader TeamInAlarmReader;
		public TeamsController Target;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakeCurrentUnitOfWorkFactory>().For<ICurrentUnitOfWorkFactory>();
			system.UseTestDouble<FakeUnitOfWorkFactory>().For<IUnitOfWorkFactory>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeNumberOfAgentsInTeamReader>().For<INumberOfAgentsInTeamReader>();
			system.UseTestDouble<FakeTeamInAlarmReader>().For<ITeamInAlarmReader>();
			system.UseTestDouble<FakeReadModelUnitOfWorkAspect>().For<IReadModelUnitOfWorkAspect>();
		}

		[Test]
		public void ShouldGetTeamsForSite()
		{
			var teamId = Guid.NewGuid();
			var team = new Team { Description = new Description("team1") }.WithId(teamId);
			var site = new Site("site").WithId();
			site.AddTeam(team);
			TeamRepository.Has(team);
			SiteRepository.Has(site);

			var result = Target.ForSite(site.Id.GetValueOrDefault()).Result<IEnumerable<TeamViewModel>>();

			result.Single().Name.Should().Be("team1");
			result.Single().Id.Should().Be(teamId);
		}

		[Test]
		public void ShouldGetNumberOfAgents()
		{
			var team = new Team { Description = new Description("t") }.WithId();
			var site = new Site("s").WithId();
			site.AddTeam(team);
			TeamRepository.Has(team);
			SiteRepository.Has(site);
			NumberOfAgentsInTeam.Has(team, 2);

			var result = Target.ForSite(site.Id.GetValueOrDefault()).Result<IEnumerable<TeamViewModel>>();

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

			Target.GetBusinessUnitId(team.Id.GetValueOrDefault()).Result<Guid>().Should().Be(businessUnit.Id.GetValueOrDefault());
		}

	}
}
