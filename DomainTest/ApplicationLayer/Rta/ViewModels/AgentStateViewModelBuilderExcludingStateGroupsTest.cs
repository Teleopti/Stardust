using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentStateViewModelBuilderExcludingStateGroupsTest
	{
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public AgentStatesViewModelBuilder Target;

		[Test]
		public void ShouldGetForTeam()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var team = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				})
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					TeamId = team,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				});
			Now.Is("2016-09-22 08:10");

			var agentState = Target.InAlarmForTeams(new[] { team}, new[] {loggedOut}).States;

			agentState.Single().PersonId.Should().Be(person1);
		}


		[Test]
		public void ShouldGetForSite()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var site = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					SiteId = site,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				})
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					SiteId = site,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				});
			Now.Is("2016-09-22 08:10");

			var agentState = Target.InAlarmForSites(new[] { site }, new[] { loggedOut }).States;

			agentState.Single().PersonId.Should().Be(person1);
		}


		[Test]
		public void ShouldGetForSkill()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				})
				.WithPersonSkill(person1, phone)
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				})
				.WithPersonSkill(person2, phone);
			Now.Is("2016-09-22 08:10");

			var agentState = Target.InAlarmForSkills(new[] { phone }, new[] { loggedOut }).States;

			agentState.Single().PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldGetWithStateGroupIdForSkill()
		{
			var person = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person,
					StateGroupId = phone,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-09-22 08:00".Utc()
				})
				.WithPersonSkill(person, skill);
			Now.Is("2016-09-22 08:10");

			var agentState = Target.InAlarmForSkills(new[] { skill }).States;

			agentState.Single().StateId.Should().Be(phone);
		}
	}
}