using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[IoCTest]
	[TestFixture]
	public class GetAgentStatesTest : ISetup
	{
		public IGetAgentStates Target;
		public FakeAgentStateReadModelReader Database;
		public MutableNow Now;
		public FakeUserTimeZone TimeZone;
		public FakeUserCulture Culture;

		public void Setup(ISystem system, IIocConfiguration configuration)
		{
			system.AddModule(new WebAppModule(configuration));
			system.UseTestDouble<FakeAgentStateReadModelReader>().For<IAgentStateReadModelReader>();
			system.UseTestDouble<MutableNow>().For<INow>();
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

			var agentState = Target.ForSites(new[] {siteId1, siteId2}, null, null);

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
				State = "state",
				StateStartTime = "2015-10-22 08:00".Utc(),
				Scheduled = "phone",
				ScheduledNext = "lunch",
				NextStart = "2015-10-22 09:00".Utc(),
				AlarmName = "in adherence",
				RuleStartTime = "2015-10-22 08:00".Utc(),
				Color = 0
			});
			Now.Is("2015-10-22 08:30".Utc());

			var agentState = Target.ForSites(new[] {siteId}, null, null);

			agentState.Single().PersonId.Should().Be(personId);
			agentState.Single().State.Should().Be("state");
			agentState.Single().StateStartTime.Should().Be("2015-10-22 08:00".Utc());
			agentState.Single().Activity.Should().Be("phone");
			agentState.Single().NextActivity.Should().Be("lunch");
			agentState.Single().NextActivityStartTime.Should().Be("09:00");
			agentState.Single().Alarm.Should().Be("in adherence");
			agentState.Single().AlarmColor.Should().Be("#000000");
			agentState.Single().TimeInState.Should().Be(30 * 60);
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
				State = "state",
				StateStartTime = "2015-10-22 08:00".Utc(),
				Scheduled = "phone",
				ScheduledNext = "lunch",
				NextStart = "2015-10-22 09:00".Utc(),
				AlarmName = "in adherence",
				RuleStartTime = "2015-10-22 08:00".Utc(),
				Color = 0
			});
			Now.Is("2015-10-22 08:30".Utc());

			var agentState = Target.ForTeams(new[] {teamId}, null, null);

			agentState.Single().PersonId.Should().Be(personId);
			agentState.Single().State.Should().Be("state");
			agentState.Single().StateStartTime.Should().Be("2015-10-22 08:00".Utc());
			agentState.Single().Activity.Should().Be("phone");
			agentState.Single().NextActivity.Should().Be("lunch");
			agentState.Single().NextActivityStartTime.Should().Be("09:00");
			agentState.Single().Alarm.Should().Be("in adherence");
			agentState.Single().AlarmColor.Should().Be("#000000");
			agentState.Single().TimeInState.Should().Be(30 * 60);
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

			var agentStates = Target.ForSites(new[] {siteId1, siteId2}, true, null);

			agentStates.Single().PersonId.Should().Be(personId1);
		}

	}
}