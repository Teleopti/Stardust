using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.AgentInfo;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ReadModelUpdaters;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[TestFixture]
	[DomainTest]
	public class GetTeamAdherenceTest : ISetup
	{
		public AgentsInAlarmForTeamsViewModelBuilder Target;
		public FakeTeamRepository Teams;
		public FakeSiteRepository Sites;
		public FakeTeamInAlarmReader OutOfAdherence;
		public FakeNumberOfAgentsInTeamReader AgentsInTeam;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble<FakeTeamRepository>().For<ITeamRepository>();
			system.UseTestDouble<FakeSiteRepository>().For<ISiteRepository>();
			system.UseTestDouble<FakeTeamInAlarmReader>().For<ITeamInAlarmReader>();
			system.UseTestDouble<FakeNumberOfAgentsInTeamReader>().For<INumberOfAgentsInTeamReader>();
		}

		[Test]
		public void ShouldReturnOutOfAdherenceCount()
		{
			var site = new Site("s").WithId();
			var team = new Team().WithId();
			team.Site = site;
			site.AddTeam(team);
			Teams.Has(team);
			Sites.Has(site);
			OutOfAdherence.Has(new AgentStateReadModel
			{
				TeamId = team.Id.Value,
				SiteId = site.Id.Value,
				IsRuleAlarm = true,
				AlarmStartTime = DateTime.MinValue
			});
			AgentsInTeam.Has(team,3);
			var result = Target.GetOutOfAdherenceForTeamsOnSite(site.Id.GetValueOrDefault()).Single();

			result.Id.Should().Be(team.Id);
			result.Name.Should().Be(team.Description.Name);
			result.NumberOfAgents.Should().Be(3);
			result.SiteId.Should().Be(site.Id.GetValueOrDefault());
			result.OutOfAdherence.Should().Be(1);
			result.Color.Should().Be("good");
		}

		[Test]
		public void ShouldReturnTeamsWithoutAdherence()
		{
			var site = new Site("s").WithId();
			var team = new Team().WithId();
			team.Site = site;
			site.AddTeam(team);
			Teams.Has(team);
			Sites.Has(site);

			var result = Target.GetOutOfAdherenceForTeamsOnSite(site.Id.GetValueOrDefault()).Single();

			result.Id.Should().Be(team.Id);
			result.Name.Should().Be(team.Description.Name);
			result.NumberOfAgents.Should().Be(0);
			result.SiteId.Should().Be(site.Id.GetValueOrDefault());
			result.Color.Should().Be(null);
			result.OutOfAdherence.Should().Be(0);
		}
	}
}