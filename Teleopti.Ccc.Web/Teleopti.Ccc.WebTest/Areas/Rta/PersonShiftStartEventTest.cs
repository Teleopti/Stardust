﻿using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture, Ignore]
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
				.WithSchedule(personId, activityId, new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc), new DateTime(2014, 10, 20, 11, 0, 0, DateTimeKind.Utc))
				.Make();
			var publisher = new FakeEventsPublisher();
			var now = new MutableNow();
			var target = new TeleoptiRtaServiceForTest(database, now, publisher);

			now.Mutate(new DateTime(2014, 10, 19, 17, 2, 0, DateTimeKind.Utc));
			target.SaveExternalUserState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "logout",
				Timestamp = new DateTime(2014, 10, 19, 17, 2, 0, DateTimeKind.Utc)
			});
			now.Mutate(new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc));
			target.GetUpdatedScheduleChange(personId, businessUnitId, new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc));

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
				.WithSchedule(personId, activityId, new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc), new DateTime(2014, 10, 20, 11, 0, 0, DateTimeKind.Utc))
				.Make();
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(database, new ThisIsNow(new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc)), publisher);

			target.GetUpdatedScheduleChange(personId, businessUnitId, new DateTime(2014, 10, 20, 10, 0, 0, DateTimeKind.Utc));

			var @event = publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}
	}
}