using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.Monitor.Unit.ViewModels.AgentStateViewModelBuilder
{
	[DomainTest]
	[TestFixture]
	public class ViewModelBuilderTest : IIsolateSystem
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		public void Isolate(IIsolate isolate)
		{
			isolate.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			isolate.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
		}

		[Test]
		public void ShouldGetAgentStatesForSites()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId2
				});

			var agentState = Target.Build(new AgentStateFilter {SiteIds = new[] {siteId1, siteId2}}).States.ToArray();

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(personId1, personId2);
		}

		[Test]
		public void ShouldGetAgentStateModelForSite()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				SiteId = siteId,
				StateName = "state",
				StateStartTime = "2015-10-22 08:00".Utc(),
				Activity = "phone",
				NextActivity = "lunch",
				NextActivityStartTime = "2015-10-22 09:00".Utc(),
				RuleName = "in adherence",
				RuleStartTime = "2015-10-22 08:00".Utc(),
				RuleColor = 0
			});
			Now.Is("2015-10-22 08:30".Utc());

			var agentState = Target.Build(new AgentStateFilter {SiteIds = new[] {siteId}}).States.Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.Activity.Should().Be("phone");
			agentState.NextActivity.Should().Be("lunch");
			agentState.NextActivityStartTime.Should().Be("09:00");
			agentState.Rule.Should().Be("in adherence");
			agentState.Color.Should().Be("#000000");
			agentState.TimeInState.Should().Be(30 * 60);
		}

		[Test]
		public void ShouldGetAgentStateModelForTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				StateName = "state",
				StateStartTime = "2015-10-22 08:00".Utc(),
				Activity = "phone",
				NextActivity = "lunch",
				NextActivityStartTime = "2015-10-22 09:00".Utc(),
				RuleName = "in adherence",
				RuleStartTime = "2015-10-22 08:00".Utc(),
				RuleColor = 0
			});
			Now.Is("2015-10-22 08:30".Utc());

			var agentState = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.Activity.Should().Be("phone");
			agentState.NextActivity.Should().Be("lunch");
			agentState.NextActivityStartTime.Should().Be("09:00");
			agentState.Rule.Should().Be("in adherence");
			agentState.Color.Should().Be("#000000");
			agentState.TimeInState.Should().Be(30 * 60);
		}

		[Test]
		public void ShouldOnlyGetAgentStatesInAlarm()
		{
			Now.Is("2016-06-15 12:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1,
					AlarmStartTime = "2016-06-15 12:00".Utc(),
					IsRuleAlarm = true
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId2,
					AlarmStartTime = null,
					IsRuleAlarm = false
				});

			var agentStates = Target.Build(new AgentStateFilter {SiteIds = new[] {siteId1, siteId2}, InAlarm = true}).States.Single();

			agentStates.PersonId.Should().Be(personId1);
		}

		[Test]
		public void ShouldGetSchedulesForTeam()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						Color = ColorTranslator.FromHtml("#80FF80").ToArgb(),
						StartTime = "2016-05-29 12:00:00".Utc(),
						EndTime = "2016-05-29 13:00:00".Utc(),
						Name = "Phone"
					}
				}
			});

			var state = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single();

			state.Shift.Single().Color.Should().Be("#80FF80");
			state.Shift.Single().StartTime.Should().Be("2016-05-29T12:00:00");
			state.Shift.Single().EndTime.Should().Be("2016-05-29T13:00:00");
			state.Shift.Single().Name.Should().Be("Phone");
		}

		[Test]
		public void ShouldGetSchedulesInUserTimeZone()
		{
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				TeamId = teamId,
				Shift = new[]
				{
					new AgentStateActivityReadModel
					{
						StartTime = "2016-05-29 12:00:00".Utc(),
						EndTime = "2016-05-29 13:00:00".Utc()
					}
				}
			});
			TimeZone.IsSweden();

			var state = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single();

			state.Shift.Single().StartTime.Should().Be("2016-05-29T14:00:00");
			state.Shift.Single().EndTime.Should().Be("2016-05-29T15:00:00");
		}

		[Test]
		public void ShouldGetCurrentTime()
		{
			Now.Is("2016-05-28 12:00");

			Target.Build(new AgentStateFilter {TeamIds = new Guid[0]})
				.Time.Should().Be("2016-05-28 12:00".Utc());
		}

		[Test]
		public void ShouldGetCurrentTimeForSitesToo()
		{
			Now.Is("2016-05-28 12:00");

			Target.Build(new AgentStateFilter {SiteIds = new Guid[0]})
				.Time.Should().Be("2016-05-28 12:00".Utc());
		}

		[Test]
		public void ShouldGetCurrentTimeInUserTimeZone()
		{
			Now.Is("2016-05-28 12:00");
			TimeZone.IsSweden();

			Target.Build(new AgentStateFilter {TeamIds = new Guid[0]})
				.Time.Should().Be("2016-05-28 14:00".Utc());
		}

		[Test]
		public void ShouldGetRuleStartTime()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				RuleStartTime = "2016-06-08 08:00".Utc()
			});
			Now.Is("2016-06-08 08:02".Utc());

			var agentState = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single();

			agentState.TimeInRule.Should().Be(120);
		}

		[Test]
		public void ShouldReturnNullIfHaveNotEnteredRuleYet()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId
			});
			Now.Is("2016-06-08 08:02".Utc());

			var agentState = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single();

			agentState.TimeInRule.Should().Be(null);
		}

		[Test]
		public void ShouldGetRecentOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 07:40".Utc(),
						EndTime = "2016-06-16 07:50".Utc()
					}
				}
			});
			Now.Is("2016-06-16 08:00");

			var outOfAdherence = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single()
				.OutOfAdherences.Single();

			outOfAdherence.StartTime.Should().Be("2016-06-16T07:40:00");
			outOfAdherence.EndTime.Should().Be("2016-06-16T07:50:00");
		}

		[Test]
		public void ShouldGetOngoingOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 07:40".Utc(),
						EndTime = null
					}
				}
			});
			Now.Is("2016-06-16 08:00");

			var outOfAdherence = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single()
				.OutOfAdherences.Single();

			outOfAdherence.StartTime.Should().Be("2016-06-16T07:40:00");
			outOfAdherence.EndTime.Should().Be(null);
		}

		[Test]
		public void ShouldGetOutOfAdherenceInUserTimeZone()
		{
			var personId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			Database.Has(new AgentStateReadModel
			{
				PersonId = personId,
				TeamId = teamId,
				OutOfAdherences = new[]
				{
					new AgentStateOutOfAdherenceReadModel
					{
						StartTime = "2016-06-16 12:00".Utc(),
						EndTime = "2016-06-16 12:10".Utc()
					}
				}
			});
			Now.Is("2016-06-16 12:30");
			TimeZone.IsSweden();

			var outOfAdherence = Target.Build(new AgentStateFilter {TeamIds = new[] {teamId}}).States.Single()
				.OutOfAdherences.Single();

			outOfAdherence.StartTime.Should().Be("2016-06-16T14:00:00");
			outOfAdherence.EndTime.Should().Be("2016-06-16T14:10:00");
		}

		[Test]
		public void ShouldGetAgentStateModelForSkill()
		{
			var person = Guid.NewGuid();
			var skill = Guid.NewGuid();
			Database
				.WithPersonSkill(person, skill)
				.Has(new AgentStateReadModel
				{
					PersonId = person
				});

			var agentState = Target.Build(new AgentStateFilter {SkillIds = new[] {skill}}).States.Single();

			agentState.PersonId.Should().Be(person);
		}

		[Test]
		public void ShouldGetAgentStateModelForSkills()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Database
				.WithPersonSkill(person1, skill1)
				.Has(new AgentStateReadModel {PersonId = person1})
				.WithPersonSkill(person2, skill2)
				.Has(new AgentStateReadModel {PersonId = person2})
				;

			var agentState = Target.Build(new AgentStateFilter {SkillIds = new[] {skill1, skill2}}).States;

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(person1, person2);
		}

		[Test]
		public void ShouldGetAgentStateInAlarmForSkill()
		{
			Now.Is("2016-06-21 08:30");
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var skill1 = Guid.NewGuid();
			var skill2 = Guid.NewGuid();
			Database
				.WithPersonSkill(person1, skill1)
				.Has(new AgentStateReadModel
				{
					PersonId = person1,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc()
				})
				.WithPersonSkill(person2, skill2)
				.Has(new AgentStateReadModel
				{
					PersonId = person2,
					IsRuleAlarm = true,
					AlarmStartTime = "2016-06-21 08:29".Utc()
				});

			var agentState = Target.Build(new AgentStateFilter {SkillIds = new[] {skill1, skill2}, InAlarm = true}).States;

			agentState.Select(x => x.PersonId).Should().Have.SameValuesAs(person1, person2);
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

			var agentState = Target.Build(new AgentStateFilter {SkillIds = new[] {skill}, InAlarm = true}).States;

			agentState.Single().StateId.Should().Be(phone);
		}
	}
}