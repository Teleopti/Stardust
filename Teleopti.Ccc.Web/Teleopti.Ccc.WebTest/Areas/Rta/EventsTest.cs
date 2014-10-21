using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class EventsTest
	{
		[Test]
		public void ShouldPublishEventsForEachPerson()
		{
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode1",
				StateCode = "statecode1",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode2",
				StateCode = "statecode2",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode1", personId1)
				.WithUser("usercode2", personId2)
				.WithSchedule(personId1, activityId, state1.Timestamp.AddHours(-1), state1.Timestamp.AddHours(1))
				.WithSchedule(personId2, activityId, state2.Timestamp.AddHours(-1), state2.Timestamp.AddHours(1))
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 1)
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state1.Timestamp), publisher);

			target.SaveExternalUserState(state1);
			target.SaveExternalUserState(state2);
			target.SaveExternalUserState(state1);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(2);
			var event1 = publisher.PublishedEvents.ElementAt(0) as PersonInAdherenceEvent;
			event1.PersonId.Should().Be(personId1);
			var event2 = publisher.PublishedEvents.ElementAt(1) as PersonOutOfAdherenceEvent;
			event2.PersonId.Should().Be(personId2);
		}

		[Test]
		public void ShouldNotPublishEventsForPersonWithNoScheduledActivity()
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state)
				.WithUser("usercode", Guid.NewGuid())
				.WithAlarm("statecode", activityId, 0)
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(0);
		}
	}
}