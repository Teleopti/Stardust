using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels
{
	[DomainTest]
	public class SiteCardViewModelBuilderTeamsTest : IIsolateSystem
	{
		public SiteCardViewModelBuilder Target;
		public FakeDatabase Database;
		public FakeSiteRepository Sites;
		public MutableNow Now;
		public FakeUserUiCulture UiCulture;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserUiCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserUiCulture>();
		}

		[Test]
		public void ShouldIncludeTeamsOfGivenSites()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId);

			var viewModel = Target.Build(null, new[] { siteId }).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Single().Id.Should().Be(teamId);
		}

		[Test]
		public void ShouldOnlyIncludeTeamsOnForGivenSites()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId);

			var viewModel = Target.Build(null, null).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Should().Be.Empty();
		}

		[Test]
		public void ShouldIncludeTeamProperties()
		{
			Now.Is("2017-03-30 08:30");
			var personId = Guid.NewGuid();
			var anotherPersonId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid(); 
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();

			
			Database
				.WithSite(siteId)
				.WithTeam(teamId, "green")
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId,
					TeamName = "green",
					IsRuleAlarm = true,
					AlarmStartTime = "2017-03-30 08:29".Utc()
				})
				.WithAgent(personId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = anotherPersonId,
					BusinessUnitId = businessUnitId,
					SiteId = siteId,
					TeamId = teamId,
					TeamName = "green",
					IsRuleAlarm = false
				})
				.WithAgent(anotherPersonId);

			var viewModel = Target.Build(null, new[] { siteId }).Sites.Single();

			viewModel.Id.Should().Be(siteId);
			viewModel.Teams.Single().Id.Should().Be(teamId);
			viewModel.Teams.Single().SiteId.Should().Be(siteId);
			viewModel.Teams.Single().Name.Should().Be("green");
			viewModel.Teams.Single().AgentsCount.Should().Be(2);
			viewModel.Teams.Single().InAlarmCount.Should().Be(1);
			viewModel.Teams.Single().Color.Should().Be("warning");
		}
		
		[Test]
		public void ShouldOrderTeamsByName()
		{
			Now.Is("2018-02-23 08:30");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var personId3 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId1 = Guid.NewGuid();
			var teamId2 = Guid.NewGuid();
			var teamId3 = Guid.NewGuid();

			Database
				.WithSite(siteId)
				.WithTeam(teamId1)
				.WithTeam(teamId2)
				.WithTeam(teamId3)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId1,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId1,
					TeamName = "C",
					IsRuleAlarm = true,
					AlarmStartTime = "2018-02-23 08:29".Utc()
				})
				.WithAgent(personId1)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId2,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId2,
					TeamName = "A",
					IsRuleAlarm = true,
					AlarmStartTime = "2018-02-23 08:29".Utc()
				})
				.WithAgent(personId2)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId3,
					BusinessUnitId = Guid.NewGuid(),
					SiteId = siteId,
					TeamId = teamId3,
					TeamName = "B",
					IsRuleAlarm = true,
					AlarmStartTime = "2018-02-23 08:29".Utc()
				})
				.WithAgent(personId3);

			var viewModel = Target.Build(null, new[] { siteId }).Sites.Single();

			viewModel.Teams.Select(x => x.Id).Should().Have.SameSequenceAs(new[]
			{
				teamId2,
				teamId3,
				teamId1
			});
		}
	}
}