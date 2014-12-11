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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 11:02");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 10:45");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 11:02");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 11:03");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 10:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 09:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 11:02");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 12:02");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);

			now.Mutate("2014-10-20 10:30");
			database.ClearSchedule(personId);
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher, dataSource);

			now.Mutate("2014-10-20 10:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 11:02");
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}
	}

}