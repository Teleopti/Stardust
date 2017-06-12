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

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class ExcludePhoneStateTest
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;

		[Test]
		public void ShouldGetForSite()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var site = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					SiteId = site,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					SiteId = site,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				});

			Target.For(new AgentStateFilter { SiteIds = new[] { site }, ExcludedStates = new Guid?[] { loggedOut } })
				.States.Single()
				.PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldGetForTeam()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var team = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					TeamId = team,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				});

			Target.For(new AgentStateFilter { TeamIds = new[] { team }, ExcludedStates = new Guid?[] { loggedOut } })
				.States.Single()
				.PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldGetForSkill()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(person1, phone)
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(person2, phone);

			Target.For(new AgentStateFilter { SkillIds = new[] { phone }, ExcludedStates = new Guid?[] { loggedOut } })
				.States.Single()
				.PersonId.Should().Be(person1);
		}

		[Test]
		public void ShouldGetForSiteAndSkill()
		{
			var expected = Guid.NewGuid();
			var stateFiltered = Guid.NewGuid();
			var skillFiltered = Guid.NewGuid();
			var siteFiltered = Guid.NewGuid();
			var site = Guid.NewGuid();
			var skill = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = expected,
					SiteId = site,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(expected, skill)
				.Has(new AgentStateReadModel
				{
					PersonId = stateFiltered,
					SiteId = site,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(stateFiltered, skill)
				.Has(new AgentStateReadModel
				{
					PersonId = skillFiltered,
					SiteId = site,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(skillFiltered, Guid.NewGuid())
				.Has(new AgentStateReadModel
				{
					PersonId = siteFiltered,
					SiteId = Guid.NewGuid(),
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(siteFiltered, skill)
				;

			Target.For(
					new AgentStateFilter
					{
						SiteIds = new[] { site },
						SkillIds = new[] { skill },
						ExcludedStates = new Guid?[] { loggedOut }
					})
				.States.Single()
				.PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldGetForTeamAndSkill()
		{
			var expected = Guid.NewGuid();
			var stateFiltered = Guid.NewGuid();
			var skillFiltered = Guid.NewGuid();
			var teamFiltered = Guid.NewGuid();
			var team = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			var loggedOut = Guid.NewGuid();
			Now.Is("2016-11-07 08:05");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = expected,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(expected, skillId)
				.Has(new AgentStateReadModel
				{
					PersonId = stateFiltered,
					TeamId = team,
					StateGroupId = loggedOut,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(stateFiltered, skillId)
				.Has(new AgentStateReadModel
				{
					PersonId = skillFiltered,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(skillFiltered, Guid.NewGuid())
				.Has(new AgentStateReadModel
				{
					PersonId = teamFiltered,
					TeamId = Guid.NewGuid(),
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(teamFiltered, skillId)
				;

			Target.For(
				new AgentStateFilter
				{
					TeamIds = new[] { team },
					SkillIds = new[] { skillId },
					ExcludedStates = new Guid?[] { loggedOut }
				})
				.States.Single()
				.PersonId.Should().Be(expected);
		}

		[Test]
		public void ShouldOrderByAlarmTime()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var site = Guid.NewGuid();
			var team = Guid.NewGuid();
			var skillId = Guid.NewGuid();
			Now.Is("2016-11-07 08:10");
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					SiteId = site,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:00".Utc()
				})
				.WithPersonSkill(person1, skillId)
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					SiteId = site,
					TeamId = team,
					StateGroupId = Guid.NewGuid(),
					IsRuleAlarm = true,
					AlarmStartTime = "2016-11-07 08:05".Utc()
				})
				.WithPersonSkill(person2, skillId)
				;

			Target.For(new AgentStateFilter { SiteIds = new[] { site }, ExcludedStates = new Guid?[] { null } })
				.States.Select(x => x.PersonId).Should().Have.SameSequenceAs(new[] { person1, person2 });

			Target.For(new AgentStateFilter { TeamIds = new[] { team }, ExcludedStates = new Guid?[] { null } })
				.States.Select(x => x.PersonId).Should().Have.SameSequenceAs(new[] { person1, person2 });

			Target.For(new AgentStateFilter { SkillIds = new[] { skillId }, ExcludedStates = new Guid?[] { null } })
				.States.Select(x => x.PersonId).Should().Have.SameSequenceAs(new[] { person1, person2 });

			Target.For(new AgentStateFilter { SiteIds = new[] { site }, SkillIds = new[] { skillId }, ExcludedStates = new Guid?[] { null } })
				.States.Select(x => x.PersonId).Should().Have.SameSequenceAs(new[] { person1, person2 });

			Target.For(new AgentStateFilter { TeamIds = new[] { team }, SkillIds = new[] { skillId }, ExcludedStates = new Guid?[] { null } })
				.States.Select(x => x.PersonId).Should().Have.SameSequenceAs(new[] { person1, person2 });
		}

	}

}