using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentsInAlarmForTeamsOnSiteViewModelBuilderTest
	{
		public FakeDatabase Database;
		public AgentsInAlarmForTeamsViewModelBuilder Target;
		public FakeNumberOfAgentsInTeamReader AgentsInTeam;
		public FakeSiteRepository Sites;
		public MutableNow Now;

		[Test]
		public void ShouldBuild()
		{
			Now.Is("2016-10-17 08:10");
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId, "Team")
				.WithAgent(personId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				})
				;
			
			var viewModel = Target.Build(siteId).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.Name.Should().Be("Team");
			viewModel.NumberOfAgents.Should().Be(1);
			viewModel.SiteId.Should().Be(siteId);
			viewModel.OutOfAdherence.Should().Be(1);
			viewModel.Color.Should().Be("danger");
		}

		[Test]
		public void ShouldBuildForSkill()
		{
			Now.Is("2016-10-17 08:10");
			var personId = Guid.NewGuid();
			var wrongPersonId = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var wrongSkill = Guid.NewGuid();
			var wrongTeam = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId,"Team")
				.WithAgent(personId)
				.WithSkill(skill)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				})
				
				.WithTeam(wrongTeam, "Team wrong")
				.WithAgent(wrongPersonId)
				.WithSkill(wrongSkill)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = wrongPersonId,
					SiteId = siteId,
					TeamId = wrongTeam,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				});

			var viewModel = Target.Build(siteId, new[] { skill }).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.Name.Should().Be("Team");
			viewModel.NumberOfAgents.Should().Be(1);
			viewModel.SiteId.Should().Be(siteId);
			viewModel.OutOfAdherence.Should().Be(1);
			viewModel.Color.Should().Be("danger");
		}

		[Test]
		public void ShouldBuildForSkillWihNoAgentsInAlarm()
		{
			var personId = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
				})
				.WithAgent(personId)
				.WithSkill(skill);

			var viewModel = Target.Build(siteId, new[] { skill }).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.OutOfAdherence.Should().Be(0);
		}


		[Test]
		public void ShouldNotCountSameAgentTwiceForSkillArea()
		{
			Now.Is("2016-10-17 08:10");
			var personId = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				})
				.WithAgent(personId)
				.WithSkill(skill1)
				.WithSkill(skill2);
	
			var viewModel = Target.Build(siteId, new[] { skill1, skill2 }).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.OutOfAdherence.Should().Be(1);
		}
	}
}