using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonActivityStartEventTest
	{
		[Test]
		public void ShouldPublishPersonShiftStartEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".ToTime(), "2014-10-20 11:00".ToTime())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-19 17:02".ToTime());
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = "2014-10-19 17:02".ToTime()
			});
			now.Mutate("2014-10-20 10:00".ToTime());
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:00".ToTime());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishShiftStartEventWhenNoPreviousState()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".ToTime(), "2014-10-20 11:00".ToTime())
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".ToTime());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishShiftStartEventWhenNextShiftStarts()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-19 10:00".ToTime(), "2014-10-19 11:00".ToTime())
				.WithSchedule(personId, activityId, "2014-10-20 10:00".ToTime(), "2014-10-20 11:00".ToTime())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-19 10:59");
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = "2014-10-19 10:59".ToTime()
			});
			now.Mutate("2014-10-20 10:00");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:00".ToTime());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Last();
			@event.ShiftStartTime.Should().Be("2014-10-20 10:00".ToTime());
		}

		[Test]
		public void ShouldPublishShiftStartEventWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00".ToTime(), "2014-10-20 10:00".ToTime())
				.WithSchedule(personId, activityId, "2014-10-20 10:00".ToTime(), "2014-10-20 11:00".ToTime())
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".ToTime());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 09:00".ToTime());
		}

		[Test]
		public void ShouldPublishShiftStartEventWithShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".ToTime(), "2014-10-20 11:00".ToTime())
				.WithSchedule(personId, activityId, "2014-10-20 11:00".ToTime(), "2014-10-20 12:00".ToTime())
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);
			
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".ToTime());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".ToTime());
		}

	}
}