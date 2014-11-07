using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonInAdherenceEventTest
	{
		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoStaffingEffect()
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
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoAlarm()
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
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishEventIfStillInAdherence()
		{
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, state1.Timestamp.AddHours(-1), state1.Timestamp.AddHours(1))
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state1.Timestamp), publisher);

			target.SaveExternalUserState(state1);
			target.SaveExternalUserState(state2);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTheTimeWhenPersonInAdherence()
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
				.WithSchedule(personId, activityId, state.Timestamp.AddHours(-1), state.Timestamp.AddHours(1))
				.WithAlarm("statecode", activityId, 0)
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(state.Timestamp), publisher);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single();
			@event.Timestamp.Should().Be(state.Timestamp);
		}

	}
}