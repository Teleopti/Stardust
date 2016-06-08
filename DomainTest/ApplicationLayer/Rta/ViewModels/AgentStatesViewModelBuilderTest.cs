using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.ViewModels
{
	[DomainTest]
	[TestFixture]
	public class AgentStatesViewModelBuilderTest : ISetup
	{
		public AgentStatesViewModelBuilder Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.UseTestDouble(new FakeUserTimeZone(TimeZoneInfo.Utc)).For<IUserTimeZone>();
			system.UseTestDouble(new FakeUserCulture(CultureInfoFactory.CreateSwedishCulture())).For<IUserCulture>();
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

			var agentState = Target.ForSites(new[] { siteId1, siteId2 }, false).States.ToArray();

			agentState.First().PersonId.Should().Be(personId1);
			agentState.Last().PersonId.Should().Be(personId2);
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

			var agentState = Target.ForSites(new[] { siteId }, false).States.Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.Activity.Should().Be("phone");
			agentState.NextActivity.Should().Be("lunch");
			agentState.NextActivityStartTime.Should().Be("09:00");
			agentState.Alarm.Should().Be("in adherence");
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

			var agentState = Target.ForTeams(new[] { teamId }, false).States.Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.Activity.Should().Be("phone");
			agentState.NextActivity.Should().Be("lunch");
			agentState.NextActivityStartTime.Should().Be("09:00");
			agentState.Alarm.Should().Be("in adherence");
			agentState.Color.Should().Be("#000000");
			agentState.TimeInState.Should().Be(30 * 60);
		}

		[Test]
		public void ShouldOnlyGetAgentStatesInAlarm()
		{
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var siteId1 = Guid.NewGuid();
			var siteId2 = Guid.NewGuid();
			Database
				.Has(new AgentStateReadModel
				{
					PersonId = personId1,
					SiteId = siteId1,
					IsRuleAlarm = true
				})
				.Has(new AgentStateReadModel
				{
					PersonId = personId2,
					SiteId = siteId2,
					IsRuleAlarm = false
				});

			var agentStates = Target.ForSites(new[] { siteId1, siteId2 }, true).States.Single();

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

			var state = Target.ForTeams(new[] { teamId }, false).States.Single();

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
						EndTime = "2016-05-29 13:00:00".Utc(),
					}
				}
			});
			TimeZone.IsSweden();

			var state = Target.ForTeams(new[] { teamId }, false).States.Single();

			state.Shift.Single().StartTime.Should().Be("2016-05-29T14:00:00");
			state.Shift.Single().EndTime.Should().Be("2016-05-29T15:00:00");
		}

		[Test]
		public void ShouldGetCurrentTime()
		{
			Now.Is("2016-05-28 12:00");

			Target.ForTeams(new Guid[] { }, false)
				.Time.Should().Be("2016-05-28 12:00".Utc());
		}

		[Test]
		public void ShouldGetCurrentTimeForSitesToo()
		{
			Now.Is("2016-05-28 12:00");

			Target.ForSites(new Guid[] { }, false)
				.Time.Should().Be("2016-05-28 12:00".Utc());
		}
		
		[Test]
		public void ShouldGetCurrentTimeInUserTimeZone()
		{
			Now.Is("2016-05-28 12:00");
			TimeZone.IsSweden();

			Target.ForTeams(new Guid[] { }, false)
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
				RuleStartTime = "2016-06-08 08:00".Utc(),
			});
			Now.Is("2016-06-08 08:02".Utc());

			var agentState = Target.ForTeams(new[] { teamId }, false).States.Single();
			
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

			var agentState = Target.ForTeams(new[] { teamId }, false).States.Single();
			
			agentState.TimeInRule.Should().Be(null);
		}
	}
}