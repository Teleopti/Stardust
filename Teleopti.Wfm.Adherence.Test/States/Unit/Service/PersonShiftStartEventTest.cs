using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonShiftStartEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			Now.Is("2014-10-19 17:02".Utc());
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			Now.Is("2014-10-20 10:00".Utc());
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfPersonNeverHasSignedIn()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:02");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWhenNextShiftStarts()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-19 10:00", "2014-10-19 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");

			Now.Is("2014-10-19 10:59");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "logout"
			});
			Now.Is("2014-10-19 11:01");
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Last();
			@event.ShiftStartTime.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 09:00", "2014-10-20 10:00")
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:02");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftStartTime.Should().Be("2014-10-20 09:00".Utc());
		}

		[Test]
		public void ShouldPublishWithShiftEndTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithSchedule(personId, activityId, "2014-10-20 11:00", "2014-10-20 12:00");
			Now.Is("2014-10-20 10:02");

			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.ShiftEndTime.Should().Be("2014-10-20 12:00".Utc());
		}
		
		[Test]
		public void ShouldPublishOnlyOneShiftStartEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2015-08-21 06:00", "2015-08-21 15:00");

			Now.Is("2015-08-21 07:00".Utc());
			Target.CheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2015-08-21 07:03".Utc());
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}
	}
}