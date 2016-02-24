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
	public class CheckForActivityChangesTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;
		public RtaTestAttribute Context;

		[Test]
		public void ShouldNoticeActivtyChanging()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;
			Now.Is("2015-09-21 09:00");

			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2015-09-21 09:00".Utc());
		}

		[Test]
		public void ShouldNoticeActivityChangingForAllPersons()
		{
			var person1 = Guid.NewGuid();
			var person2 = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("user1", person1)
				.WithSchedule(person1, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithUser("user2", person2)
				.WithSchedule(person2, phone, "2015-09-21 09:02", "2015-09-21 11:00")
				;
			Now.Is("2015-09-21 09:02");

			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single(x => x.PersonId == person1).StartTime.Should().Be("2015-09-21 09:00".Utc());
			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single(x => x.PersonId == person2).StartTime.Should().Be("2015-09-21 09:02".Utc());
		}

		[Test]
		public void ShouldNotUpdateWhenActivityHasNotChanged()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:02");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.ReceivedTime.Should().Be("2015-09-21 09:00".Utc());
		}

		[Test]
		public void ShouldNoticeActivityChangingToNextActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-09-21 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2015-09-21 11:00".Utc());
		}


		[Test]
		public void ShouldNoticeActivityChangingToNoActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-09-21 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single().ShiftEndTime.Should().Be("2015-09-21 11:00".Utc());
		}

		[Test]
		public void ShouldNotUpdateWhenStillNoActivity()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("user", person)
				.WithSchedule(person, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 11:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 11:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.ReceivedTime.Should().Be("2015-09-21 11:00".Utc());
		}

		[Test]
		public void ShouldNoticeScheduleRemoval()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database.ClearSchedule(personId);
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.ReceivedTime.Should().Be("2015-09-21 09:05".Utc());
		}

		[Test]
		public void ShouldNoticeScheduleChange()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearSchedule(personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 12:00")
				.WithSchedule(personId, lunch, "2015-09-21 12:00", "2015-09-21 13:00")
				;
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.NextActivityStartTime.Should().Be("2015-09-21 12:00".Utc());
		}

		[Test]
		public void ShouldNoticeScheduleChangeInThePast()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearSchedule(personId)
				.WithSchedule(personId, phone, "2015-09-21 08:55", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.ReceivedTime.Should().Be("2015-09-21 09:05".Utc());
		}

		[Test]
		public void ShouldNoticeScheduleActivityChange()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearSchedule(personId)
				.WithSchedule(personId, admin, "2015-09-21 09:00", "2015-09-21 11:00")
				;
			Target.ReloadSchedulesOnNextCheckForActivityChanges(Database.TenantName(), personId);
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.ActivityId.Should().Be(admin);
		}

		[Test]
		public void ShouldUpdateWithCorrectAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var alarm = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithRule("phone", phone, alarm)
				;

			Now.Is("2015-09-21 09:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Context.SimulateRestart();
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.RuleId.Should().Be(alarm);
		}
	}
}