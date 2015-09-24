using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class PersonShiftEndEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public FakeCurrentDatasource dataSource;
		public Domain.ApplicationLayer.Rta.Service.Rta target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-20 10:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 11:02");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishWhenCurrentlyInShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-20 10:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 10:45");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldPublishEventOnce()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-20 10:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 11:02");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 11:03");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 10:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-20 09:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 11:02");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 09:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00");

			now.Is("2014-10-20 10:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 12:02");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".Utc());
		}

		[Test]
		public void ShouldPublishOnTheEndingMinute()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 10:00")
				;

			now.Is("2014-10-20 8:00");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 10:00");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 8:00".Utc());
			@event.ShiftEndTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test, Ignore]
		public void ShouldPublishWithCurrentTimeAsShiftEndTimeWhenShiftIsRemoved()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-20 10:01");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			now.Is("2014-10-20 10:30");
			Database.ClearSchedule(personId);
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 10:00".Utc());
			@event.ShiftEndTime.Should().Be("2014-10-20 10:30".Utc());
		}
		
		[Test]
		public void ShouldPublishEventWithPreviousShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-19 10:00", "2014-10-19 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			now.Is("2014-10-19 10:30");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			now.Is("2014-10-20 11:02");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Last();
			@event.ShiftEndTime.Should().Be("2014-10-20 11:00".Utc());
		}
		
	}

}