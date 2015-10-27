using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[RtaTest]
	[TestFixture]
	public class SendAgentStateTest
	{
		public FakeRtaDatabase Database;
		public FakeMessageSender Sender;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldSend()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			Database.WithDataFromState(state);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
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
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database 
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId, businessUnitId)
				;
			Now.Is("2014-10-20 10:00");

			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.Should().Not.Be.Null();
		}

		[Test]
		public void ShouldSendWithReceivedSystemTime()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			Database
				.WithDataFromState(state);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ReceivedTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldSendWithCurrentActivity()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(activityId);
		}

		[Test]
		public void ShouldNotSendWithCurrentActivityIfNoSchedule()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			Database
				.WithDataFromState(state);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(null);
		}

		[Test]
		public void ShouldNotSendWithCurrentActivityIfFutureSchedule()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledId.Should().Be(null);
		}

		[Test]
		public void ShouldSendWithNextActivity()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(activityId);
		}

		[Test]
		public void ShouldNotSendWithNextActivityFromNextShift()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-21 10:00", "2014-10-21 11:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(null);
		}

		[Test]
		public void ShouldSendWithNextActivityFromFutureShift()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				;
			Now.Is("2014-10-20 10:00");

			Target.SaveState(state);

			var sent = Sender.NotificationOfType<AgentStateReadModel>().DeseralizeActualAgentState();
			sent.ScheduledNextId.Should().Be(activityId);
		}


		[Test]
		public void ShouldSendWithCorrectAlarmStartWhenChangingAlarm()
		{
			var personId = Guid.NewGuid();
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			};
			var phoneId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(state1)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phoneId, "phone", "2015-01-13 10:00", "2015-01-13 11:00")
				.WithAlarm("phone", phoneId, TimeSpan.FromMinutes(10))
				.WithAlarm("break", phoneId, TimeSpan.FromMinutes(10))
				;
			Now.Is("2015-01-13 10:00");
			Target.SaveState(state1);
			Target.SaveState(state2);

			Target.SaveState(state1);
			
			Sender.NotificationsOfType<AgentStateReadModel>()
				.Last().DeseralizeActualAgentState()
				.AlarmStart.Should().Be("2015-01-13 10:10".Utc());
		}
	}
}