using System;
using System.Drawing;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Events;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.RealTimeAdherence.Domain.Service
{
	[TestFixture]
	[RtaTest]
	public class LateForWorkTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Ccc.Domain.RealTimeAdherence.Domain.Service.Rta Target;

		[Test]
		public void ShouldPublishWhenInAdherenceAfterAlarmFromShiftStart()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithMappedRule("loggedOut", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("phone", phone, Adherence.In);
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

			Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>()
				.Single()
				.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishWithShiftStartTime()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithMappedRule("loggedOut", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("phone", phone, Adherence.In);
			
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

			Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>()
				.Single()
				.ShiftStart.Should().Be("2018-05-30 8:00".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenLateForWorkWithinAlarmThreshold()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithMappedRule("loggedOut", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("phone", phone, Adherence.In);
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});

			Now.Is("2018-05-30 08:04");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>()
				.Should().Be.Empty();
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, "Phone", phone, Color.Aqua, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithMappedRule("loggedOut", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("phone", phone, 0, "InAdherence", Adherence.In, Color.BlueViolet);
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

			var @event = Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>().Single();

			@event.ActivityColor.Should().Be(Color.Aqua.ToArgb());
			@event.ActivityName.Should().Be("Phone");
			@event.Adherence.Should().Be(EventAdherence.In);
			@event.RuleColor.Should().Be(Color.BlueViolet.ToArgb());
			@event.RuleName.Should().Be("InAdherence");
			@event.ShiftStart.Should().Be("2018-05-30 8:00".Utc());
			@event.StateName.Should().Be("phone");
			@event.Timestamp.Should().Be("2018-05-30 09:00".Utc());
		}

		[Test]
		public void ShouldNotPublishWhenStillInAlarm()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2018-05-30 8:00", "2018-05-30 17:00")
				.WithMappedRule("loggedOut", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("notReady", phone, Adherence.Out)
				.WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("phone", phone, Adherence.In);
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
				StateCode = "notReady"
			});

			Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>().FirstOrDefault().Should().Be(null);
		}
	}
}