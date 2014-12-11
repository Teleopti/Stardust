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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-19 17:02".Utc());
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Mutate("2014-10-20 10:00".Utc());
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-19 10:00", "2014-10-19 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);

			now.Mutate("2014-10-19 10:59");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Mutate("2014-10-19 11:01");
			target.CheckForActivityChange(personId, businessUnitId);
			now.Mutate("2014-10-20 10:00");
			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 10:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:02"), publisher);

			target.CheckForActivityChange(personId, businessUnitId);

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
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var dataSource = new FakeCurrentDatasource("datasource");
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher, dataSource);

			now.Mutate("2014-10-19 17:02".Utc());
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			now.Mutate("2014-10-20 10:00".Utc());
			target.CheckForActivityChange(personId, businessUnitId);

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

	}
}