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
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class LateForWorkEventTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublish()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 08:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Should().Not.Be.Empty();
		}

		[Test]
		public void ShouldPublishWithShiftStartAndTimestamp()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 08:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Single();
			@event.ShiftStart.Should().Be("2018-05-30 08:00".Utc());
			@event.Timestamp.Should().Be("2018-05-30 08:30".Utc());
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, "Phone", phone, Color.Aqua, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithMappedRule("phone", phone, 0, "InAdherence", Adherence.Configuration.Adherence.In, Color.BlueViolet)
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut");
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 09:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Single();

			@event.PersonId.Should().Be(personId);
			@event.ActivityColor.Should().Be(Color.Aqua.ToArgb());
			@event.ActivityName.Should().Be("Phone");
			@event.Adherence.Should().Be(EventAdherence.In);
			@event.RuleColor.Should().Be(Color.BlueViolet.ToArgb());
			@event.RuleName.Should().Be("InAdherence");
			@event.StateName.Should().Be("Phone");
		}

		[Test]
		public void ShouldNotPublishWhenNotLateForWork()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 08:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldOnlyPublishWhenBackFromLateFromWork()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});

			Now.Is("2018-05-30 08:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2018-05-30 08:45");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 09:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Single();
			@event.ShiftStart.Should().Be("2018-05-30 08:00".Utc());
			@event.Timestamp.Should().Be("2018-05-30 08:30".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenWithin1MinuteThreshold()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 08:00:59");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenLateOnSecondActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var email = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithAssignedActivity(email, "2018-05-30 08:00", "2018-05-30 09:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 09:30");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Single();
			@event.ShiftStart.Should().Be("2018-05-30 08:00".Utc());
			@event.Timestamp.Should().Be("2018-05-30 09:30".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenArrivingEarlyAfterSkippingOneDay()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithSchedule(personId, phone, "2018-05-31 08:00", "2018-05-31 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-31 07:58:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWhenLateTheFirstDay()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 08:00", "2018-05-30 17:00")
				.WithStateGroup("Phone").WithStateCode("phone")
				.WithLoggedOutStateGroup("Logged Out").WithStateCode("loggedOut")
				;

			Now.Is("2018-05-30 08:30");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivedLateForWorkEvent>().Should().Not.Be.Empty();
		}
	}
}