using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SendAgentStateWithStateTest
	{
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public FakeMessageSender Sender;
		public FakeRtaDatabase Database;
		public MutableNow Now;

		[Test]
		public void ShouldSendWithState()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, "my state");
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.State.Should().Be("my state");
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChanged()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			Database.WithUser("usercode");
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);
			Target.SaveState(state);

			var sent = Sender.AllNotifications.Where(n => n.DomainType == typeof(AgentStateReadModel).Name);
			sent.Should().Have.Count.EqualTo(1);
		}

		[Test, Ignore]
		public void ShouldNotSendWhenAgentStateIsNull()
		{
			Assert.Fail();
		}

		[Test]
		public void ShouldSendWithStateStartFromSystemTime()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithAlarm("statecode", activityId, 0)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:01");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.StateStart.Should().Be.EqualTo("2014-10-20 10:01".Utc());
		}

	}
}