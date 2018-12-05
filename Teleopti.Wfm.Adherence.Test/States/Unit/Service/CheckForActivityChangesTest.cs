using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.ApplicationLayer.ReadModels;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.Monitor;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class CheckForActivityChangesTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;
		public RtaTestAttribute Context;
		public FakeCurrentScheduleReadModelPersister Schedules;

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange1()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithStateCode("phone");
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldKeepPreviousStateWhenNotifiedOfActivityChange2()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 9:00", "2014-10-20 11:00")
				.WithMappedRule("phone", activityId, "alarm")
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldHandleUnrecognizedPersonId()
		{
			Assert.DoesNotThrow(() => Target.CheckForActivityChanges(Database.TenantName(), Guid.NewGuid()));
		}

		[Test]
		public void ShouldNoticeActivtyChanging()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
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
				.WithAgent("user1", person1)
				.WithSchedule(person1, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithAgent("user2", person2)
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
				.WithAgent("usercode", personId)
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 10:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 10:55");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-09-21 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2015-09-21 11:00".Utc());
		}

		[Test]
		public void ShouldNoticeShiftEnding()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 10:55");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-09-21 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single().ShiftEndTime.Should().Be("2015-09-21 11:00".Utc());
		}


		[Test]
		public void ShouldNoticeShiftStarting()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 10:00")
				;

			Now.Is("2015-09-21 08:55");
			Target.CheckForActivityChanges(Database.TenantName());
			Publisher.Clear();
			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single().ShiftStartTime.Should().Be("2015-09-21 09:00".Utc());
			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2015-09-21 09:00".Utc());
		}

		[Test]
		public void ShouldNotUpdateWhenStillNoActivity()
		{
			var person = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("user", person)
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database.ClearAssignments(personId);
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithSchedule(personId, lunch, "2015-09-21 11:00", "2015-09-21 12:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearAssignments(personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 12:00")
				.WithSchedule(personId, lunch, "2015-09-21 12:00", "2015-09-21 13:00")
				;
			Publisher.Clear();
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<AgentStateChangedEvent>().Single()
				.NextActivityStartTime.Should().Be("2015-09-21 12:00".Utc());
		}

		[Test]
		public void ShouldNoticeScheduleActivityChange()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				;

			Now.Is("2015-09-21 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2015-09-21 09:05");
			Database
				.ClearAssignments(personId)
				.WithSchedule(personId, admin, "2015-09-21 09:00", "2015-09-21 11:00")
				;
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
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2015-09-21 09:00", "2015-09-21 11:00")
				.WithMappedRule("phone", phone, alarm)
				;

			Now.Is("2015-09-21 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Context.SimulateRestart();
			Target.CheckForActivityChanges(Database.TenantName());

			Database.StoredState.RuleId.Should().Be(alarm);
		}

		[Test]
		public void ShouldNotFailWhenReceivingMultipleSchedules()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "Phone", "2017-11-20 09:00", "2017-11-20 11:00")
				;
			Schedules.Has(personId, 1000, new[]
			{
				new ScheduledActivity
				{
					BelongsToDate = "2017-11-20".Date(),
					PersonId = personId,
					Name = "Phone",
					StartDateTime = "2017-11-20 09:00".Utc(),
					EndDateTime = "2017-11-20 11:00".Utc(),
				},
			});

			Now.Is("2017-11-20 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<AgentStateChangedEvent>().Single()
				.CurrentActivityName.Should().Be("Phone");
		}
	}
}