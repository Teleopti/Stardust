using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonShiftEndEventTest
	{
		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());
			now.Mutate("2014-10-20 11:02");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 11:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishWhenCurrentlyInShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());
			now.Mutate("2014-10-20 10:45");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:45".Utc());

			publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldPublishEventOnce()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());
			now.Mutate("2014-10-20 11:02");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 11:02".Utc());
			now.Mutate("2014-10-20 11:03");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 11:03".Utc());

			publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00".Utc(), "2014-10-20 10:00".Utc())
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 09:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 09:01".Utc());
			now.Mutate("2014-10-20 11:02");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 11:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 09:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.WithSchedule(personId, activityId, "2014-10-20 11:00".Utc(), "2014-10-20 12:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());
			now.Mutate("2014-10-20 12:02");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 12:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".Utc());
		}

		[Test]
		public void ShouldSetShiftEndTimeToRecievedTimeWhenScheduleIsRemoved()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());

			now.Mutate("2014-10-20 10:30");
			database.ClearSchedule(personId);
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:30".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 10:30".Utc());
		}

		[Test]
		public void ShouldPublishEventWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher, dataSource);

			now.Mutate("2014-10-20 10:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:01".Utc());
			now.Mutate("2014-10-20 11:02");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 11:02".Utc());

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}
	}

}