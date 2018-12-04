using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonAdherenceDayStartEventTest
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
				.WithSchedule(personId, activityId, "2018-10-19 10:00", "2018-10-19 11:00");

			Now.Is("2018-10-19 09:00".Utc());
			Target.CheckForActivityChanges(Database.TenantName());

			var @event = Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishBeforeOneHourBeforeShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2018-10-19 10:00", "2018-10-19 11:00");

			Now.Is("2018-10-19 08:59".Utc());
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWithTime()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2018-10-19 10:00", "2018-10-19 11:00");

			Now.Is("2018-10-19 09:00".Utc());
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldPublishOneHourBeforeShift()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2018-10-19 10:00", "2018-10-19 11:00");

			Now.Is("2018-10-19 08:59");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2018-10-19 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldNotPublishBeforeNextActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-10-19 10:00", "2018-10-19 20:00")
				.WithSchedule(personId, lunch, "2018-10-19 12:00", "2018-10-19 13:00");

			Now.Is("2018-10-19 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2018-10-19 11:00");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldPublishOnce()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-10-19 10:00", "2018-10-19 20:00");

			Now.Is("2018-10-19 09:00");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2018-10-19 09:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldPublishForOneHourBeforeShift()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-10-19 10:00", "2018-10-19 20:00");

			Now.Is("2018-10-19 08:59");
			Target.CheckForActivityChanges(Database.TenantName());
			Now.Is("2018-10-19 09:01");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldPublishForOneHourBeforeShiftInThePast()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-10-19 10:00", "2018-10-19 20:00");

			Now.Is("2018-10-19 09:30");
			Target.CheckForActivityChanges(Database.TenantName());

			Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single()
				.Timestamp.Should().Be("2018-10-19 09:00".Utc());
		}

		[Test]
		public void ShouldHaveSolidProof()
		{
			var personId = Guid.NewGuid();
			var state = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2018-10-19 10:00", "2018-10-19 11:00")
				.WithStateGroup(state, "state")
				.WithStateCode("statecode")
				.WithMappedRule("statecode", null, 0, "rule", Adherence.Configuration.Adherence.Out, Color.Pink)
				;
			Now.Is("2018-10-18 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			Now.Is("2018-10-19 09:00");
			Target.CheckForActivityChanges(Database.TenantName());

			var @event = Publisher.PublishedEvents.OfType<PersonAdherenceDayStartEvent>().Single();
			@event.StateName.Should().Be("state");
			@event.ActivityName.Should().Be.Null();
			@event.ActivityColor.Should().Be(null);
			@event.RuleName.Should().Be("rule");
			@event.RuleColor.Should().Be(Color.Pink.ToArgb());
			@event.Adherence.Should().Be(EventAdherence.Out);
		}
	}
}