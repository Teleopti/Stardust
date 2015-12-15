using System;
using System.Linq;
using NUnit.Framework;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModelBuilders;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.IocCommon;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeData;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Core.IoC;
using Teleopti.Interfaces.Domain;
using SharpTestsEx;

namespace Teleopti.Ccc.WebTest.Areas.Anywhere.Rta
{
	[IoCTest]
	[TestFixture]
	public class AgentStateViewModelBuilderTest : ISetup
	{
		public IAgentStateViewModelBuilder Target;
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
		public void ShouldMapAgentStatesToViewModel()
		{
			var personId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var agentStates = new[]
			{
				new AgentStateReadModel
				{
					PersonId = personId,
					SiteId = siteId,
					TeamId = teamId,
					State = "state",
					StateStartTime = "2015-10-22 08:00".Utc(),
					Scheduled = "phone",
					ScheduledNext = "lunch",
					NextStart = "2015-10-22 09:00".Utc(),
					AlarmName = "in adherence",
					RuleStartTime = "2015-10-22 08:00".Utc(),
					AlarmStartTime = "2015-10-22 08:05".Utc(),
					Color = 0
				}
			};
			Now.Is("2015-10-22 08:30".Utc());

			var states = Target.Build(agentStates);

			states.Single().PersonId.Should().Be(personId);
			states.Single().State.Should().Be("state");
			states.Single().StateStartTime.Should().Be("2015-10-22 08:00".Utc());
			states.Single().Activity.Should().Be("phone");
			states.Single().NextActivity.Should().Be("lunch");
			states.Single().NextActivityStartTime.Should().Be("09:00");
			states.Single().Alarm.Should().Be("in adherence");
			states.Single().AlarmStart.Should().Be("2015-10-22 08:00".Utc());
			states.Single().AlarmColor.Should().Be("#000000");
			states.Single().TimeInState.Should().Be(30 * 60);
		}
		
		[Test]
		public void ShouldHaveWhiteAsDefaultColor()
		{
			var agentStates = new[]
			{
				new AgentStateReadModel()
			};

			var states = Target.Build(agentStates);

			states.Single().AlarmColor.Should().Be("#FFFFFF");
		}

		[Test]
		public void ShouldGetActivityTimeInLoggedOnUserTimeZone()
		{
			var agentStates = new[]
			{
				new AgentStateReadModel
				{
					NextStart = "2015-11-23 09:00".Utc()
				}
			};
			Now.Is("2015-11-23 08:30".Utc());
			TimeZone.IsSweden();
			Culture.IsSwedish();

			var states = Target.Build(agentStates);

			states.Single().NextActivityStartTime.Should().Be("10:00");
		}

		[Test]
		public void ShouldGetActivityTimeForTomorrow()
		{
			var agentStates = new[]
			{
				new AgentStateReadModel
				{
					NextStart = "2015-11-24 09:00".Utc()
				}
			};
			Now.Is("2015-11-23 17:30".Utc());
			TimeZone.IsSweden();
			Culture.IsSwedish();

			var states = Target.Build(agentStates);

			states.Single().NextActivityStartTime.Should().Be("2015-11-24 10:00");
		}

		[Test]
		public void ShouldGetNullActivityTime()
		{
			var agentStates = new[]
			{
				new AgentStateReadModel
				{
					NextStart = null
				}
			};
			Now.Is("2015-11-23 08:30".Utc());

			var states = Target.Build(agentStates);

			states.Single().NextActivityStartTime.Should().Be(null);
		}
		
		[Test]
		public void ShouldHaveAlarmStartFromAlarmStartTime()
		{
			var agentStates = new[]
			{
				new AgentStateReadModel
				{
					AlarmStartTime = "2015-11-23 08:15".Utc()
				}
			};

			var states = Target.Build(agentStates);

			states.Single().AlarmStart.Should().Be("2015-11-23 08:15".Utc());
		}
	}
}