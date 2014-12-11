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
	public class SendAgentStateWithStateTest
	{
		[Test]
		public void ShouldSendWithState()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00".Utc(), "2014-10-20 11:00".Utc())
				.WithAlarm("statecode", activityId, "my state")
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), sender);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			});

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.State.Should().Be("my state");
		}

		[Test]
		public void ShouldNotSendWhenStateHaveNotChanged()
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
			target.SaveState(state);

			var sent = sender.AllNotifications.Where(n => n.DomainType == typeof(IActualAgentState).Name);
			sent.Should().Have.Count.EqualTo(1);
		}

		[Test, Ignore]
		public void ShouldNotSendWhenAgentStateIsNull()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldAddStateCodeToDatabaseWhenNotRecognized()
		{
			Assert.Fail();
		}

		[Test, Ignore]
		public void ShouldUseDefaultStateGroupIfStateCodeIsNotRecognized()
		{
			Assert.Fail();
		}

		[Test]
		public void ShouldSendWithStateStartFromSystemTime()
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
				.WithAlarm("statecode", activityId, 0)
				.WithSchedule(personId, activityId, "2014-10-20 9:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var sender = new FakeMessageSender();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:01"), sender);

			target.SaveState(state);

			var sent = sender.NotificationOfType<IActualAgentState>().DeseralizeActualAgentState();
			sent.StateStart.Should().Be.EqualTo("2014-10-20 10:01".Utc());
		}

	}
}