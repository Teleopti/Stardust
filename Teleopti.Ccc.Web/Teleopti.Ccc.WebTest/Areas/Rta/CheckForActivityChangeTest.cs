using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Infrastructure.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class CheckForActivityChangeTest
	{
		[Test]
		public void ShouldKeepPreviousStateCodeWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId, businessUnitId)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
				Timestamp = "2014-10-20 10:00".Utc()
			});
			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var sent = sender.NotificationsOfType<IActualAgentState>().Last().DeseralizeActualAgentState();
			sent.StateCode.Should().Be("phone");
		}

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00".Utc(), "2014-10-20 11:00".Utc())
				.WithAlarm("phone", activityId, "alarm")
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
				Timestamp = "2014-10-20 10:00".Utc()
			});
			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var sent = sender.NotificationsOfType<IActualAgentState>().Last().DeseralizeActualAgentState();
			sent.State.Should().Be("alarm");
		}

		[Test, Ignore]
		public void ShouldNotifyWithActivityStartTimeAsTimestamp()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00".Utc(), "2014-10-20 11:00".Utc())
				.WithAlarm("phone", activityId, "alarm")
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:05"), sender);

			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 09:06".Utc());

			var sent = sender.NotificationsOfType<IActualAgentState>().Last().DeseralizeActualAgentState();
			sent.ReceivedTime.Should().Be("2014-10-20 9:00".Utc());
		}

		[Test, Ignore]
		public void ShouldNotUpdateTimestempIfStateChangeOccurredBeforeCheck()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00".Utc(), "2014-10-20 11:00".Utc())
				.WithAlarm("phone", activityId, "alarm")
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:02"), sender);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
				Timestamp = "2014-10-20 9:01".Utc()
			});
			sender.AllNotifications.Clear();
			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 9:03".Utc());

			var sent = sender.NotificationsOfType<IActualAgentState>();
			sent.Should().Have.Count.EqualTo(0);
			database.PersistedActualAgentState.ReceivedTime.Should().Be("2014-10-20 9:01".Utc());
		}
	}
}