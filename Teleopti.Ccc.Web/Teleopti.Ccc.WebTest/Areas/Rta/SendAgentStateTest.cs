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
	public class SendAgentStateTest
	{
		[Test]
		public void ShouldSend()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var database = new FakeRtaDatabase()
				.WithDataFromState(state)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.Should().Not.Be.Null();
		}

		[Test, Ignore]
		public void ShouldNotSendWhenWrongDataSource()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldNotSendWhenWrongPerson()
		{
			Assert.Fail();
		}

		[Test]
		public void ShouldSendWhenNotifiedOfPossibleScheduleChange()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnitId)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSendWithReceivedSystemTime()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var database = new FakeRtaDatabase()
				.WithDataFromState(state)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:01"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ReceivedTime.Should().Be("2014-10-20 10:01".Utc());
		}

		[Test]
		public void ShouldSendWithCurrentActivity()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(activityId);
		}

		[Test]
		public void ShouldNotSendWithCurrentActivityIfNoSchedule()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var database = new FakeRtaDatabase()
				.WithDataFromState(state)
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(Guid.Empty);
		}

		[Test]
		public void ShouldNotSendWithCurrentActivityIfFutureSchedule()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00".Utc(), "2014-10-20 12:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(Guid.Empty);
		}

		[Test]
		public void ShouldSendWithNextActivity()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.WithSchedule(personId, activityId, "2014-10-20 11:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(activityId);
		}

		[Test]
		public void ShouldNotSendWithNextActivityFromNextShift()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-21 10:00".Utc(), "2014-10-21 11:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(Guid.Empty);
		}

		[Test]
		public void ShouldSendWithNextActivityFromFutureShift()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00".Utc(), "2014-10-20 12:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(activityId);
		}
	}
}