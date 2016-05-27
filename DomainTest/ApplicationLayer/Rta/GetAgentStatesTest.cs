using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[DomainTest]
	[TestFixture]
	public class GetAgentStatesTest : ISetup
	{
		public IJsonSerializer Serializer;
		public IGetAgentStates Target;
		public FakeAgentStateReadModelPersister Database;
		public MutableNow Now;

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

			var agentState = Target.ForSites(new[] {siteId1, siteId2}, false).ToArray();

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

			var agentState = Target.ForSites(new[] {siteId}, false).Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.StateStartTime.Should().Be("2015-10-22 08:00".Utc());
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

			var agentState = Target.ForTeams(new[] {teamId}, false).Single();

			agentState.PersonId.Should().Be(personId);
			agentState.State.Should().Be("state");
			agentState.StateStartTime.Should().Be("2015-10-22 08:00".Utc());
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

			var agentStates = Target.ForSites(new[] {siteId1, siteId2}, true).Single();

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
				Shift = Serializer.SerializeObject(new
				{
					Color = "#80FF80",
					Offset = "10%",
					Width = "25%"
				})
			});

			var state = Target.ForTeams(new[] {teamId}, false).Single();

			state.Shift
				.Should().Be(
					Serializer.SerializeObject(new
					{
						Color = "#80FF80",
						Offset = "10%",
						Width = "25%"
					}));
		}
	}
}