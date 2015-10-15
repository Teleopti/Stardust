using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SendAgentStateWithAlarmTest
	{
		public FakeRtaDatabase Database;
		public MutableNow Now;
		public FakeMessageSender Sender;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldSendWithAlarm()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var alarmId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, alarmId)
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState()
				.AlarmId.Should().Be(alarmId);
		}

		[Test, Ignore]
		public void ShouldNotSendAlarmIfNoAlarm()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldSendAlarmForNoStateWhenNoStateGroup()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldSendAlarmForNoScheudledAcitivtyWhenNoScheduledActivity()
		{
			Assert.Fail();
		}

	}
}