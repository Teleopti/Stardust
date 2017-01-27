using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentsInAlarmForTeamsOnSiteViewModelBuilderTest
	{
		public FakeDatabase Database;
		public AgentsInAlarmForTeamsViewModelBuilder Target;
		public FakeSiteRepository Sites;
		public MutableNow Now;
		
		[Test]
		public void ShouldBuildForSkill()
		{
			Now.Is("2016-10-17 08:10");
			var personId = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database
				.WithSite(siteId)
				.WithTeam(teamId)
				.WithAgentState_DontUse(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				})
				.OnSkill_DontUse(skill);

			var viewModel = Target.ForSkills(siteId, new[] { skill }).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.OutOfAdherence.Should().Be(1);
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
				.WithAgentState_DontUse(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
				})
				.OnSkill_DontUse(skill);

			var viewModel = Target.ForSkills(siteId, new[] { skill }).Single();

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
				.WithAgentState_DontUse(new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-10-17 08:00".Utc(),
				})
				.OnSkill_DontUse(skill1)
				.OnSkill_DontUse(skill2)
				;

			var viewModel = Target.ForSkills(siteId, new[] { skill1, skill2 }).Single();

			viewModel.Id.Should().Be(teamId);
			viewModel.OutOfAdherence.Should().Be(1);
		}
	}
}