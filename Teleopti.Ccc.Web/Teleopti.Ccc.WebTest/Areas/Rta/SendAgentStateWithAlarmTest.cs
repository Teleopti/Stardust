using System;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class SendAgentStateWithAlarmTest
	{
		[Test]
		public void ShouldSendWithAlarm()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var alarmId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.WithAlarm("statecode", activityId, alarmId)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.AlarmId.Should().Be(alarmId);
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