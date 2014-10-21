using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class PersonOutOfAdherenceEventTest
	{
		[Test]
		public void ShouldPublishPersonOutOfAdherenceEventOnPositiveStaffingEffect()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, 1)
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.Single() as PersonOutOfAdherenceEvent;
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishPersonOutOfAdherenceEventOnNegativeStaffingEffect()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", personId)
				.WithSchedule(activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, -1)
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.Single() as PersonOutOfAdherenceEvent;
			@event.PersonId.Should().Be(personId);
		}

	}
}