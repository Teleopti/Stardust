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
	public class PersonShiftStartEventTest
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

			now.Mutate("2014-10-19 17:02".Utc());
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = "2014-10-19 17:02".Utc()
			});
			now.Mutate("2014-10-20 10:00".Utc());
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfPersonNeverHasSignedIn()
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
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWhenNextShiftStarts()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-19 10:00".Utc(), "2014-10-19 11:00".Utc())
				.WithSchedule(personId, activityId, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate("2014-10-19 10:59");
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = "2014-10-19 10:59".Utc()
			});
			now.Mutate("2014-10-19 11:01");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-19 11:01".Utc());
			now.Mutate("2014-10-20 10:00");
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Last();
			@event.ShiftStartTime.Should().Be("2014-10-20 10:00".Utc());
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
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
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
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:02".Utc());

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".Utc());
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

			now.Mutate("2014-10-19 17:02".Utc());
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = "2014-10-19 17:02".Utc()
			});
			now.Mutate("2014-10-20 10:00".Utc());
			target.GetUpdatedScheduleChange(personId, businessUnitId, "2014-10-20 10:00".Utc());

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

	}
}