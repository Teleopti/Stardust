using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonShiftEndEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

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

			Now.Is("2014-10-20 10:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 11:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
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

			Now.Is("2014-10-20 10:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:45");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(0);
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

			Now.Is("2014-10-20 10:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 11:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 11:03");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Should().Have.Count.EqualTo(1);
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

			Now.Is("2014-10-20 09:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 11:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
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

			Now.Is("2014-10-20 10:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 12:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
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

			Now.Is("2014-10-20 8:00");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:00");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
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

			Now.Is("2014-10-20 10:01");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			Now.Is("2014-10-20 10:30");
			Database.ClearSchedule(personId);
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single();
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

			Now.Is("2014-10-19 10:30");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 11:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Last();
			@event.ShiftEndTime.Should().Be("2014-10-20 11:00".Utc());
		}

		[Test]
		public void ShouldNotPublishEventWhenCheckingForActivityChangesTooLateSoItCantFindTheShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2015-10-05 9:00", "2015-10-05 17:00")
				;

			Now.Is("2015-10-05 9:30");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-10-08 8:00");
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().SingleOrDefault();
			@event.Should().Be.Null();
		}
	}

}