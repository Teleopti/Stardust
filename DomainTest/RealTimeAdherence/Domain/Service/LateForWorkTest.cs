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
		[Ignore("Maybe implement later")]
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
		[Ignore("Maybe implement later")]
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
		[Ignore("Maybe implement later")]
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
		[Ignore("Maybe implement later")]
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
		[Ignore("Maybe implement later")]
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

		[Test]
		[Ignore("Maybe implement later")]
		public void ShouldPublishWhenArrivingLateOnFirstBreak()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithAssignment(personId, "2018-05-30")
				.WithActivity(phone)
				.WithAssignedActivity("2018-05-30 8:00", "2018-05-30 17:00")
				.WithActivity(brejk)
				.WithAssignedActivity("2018-05-30 10:00", "2018-05-30 10:15")
				.WithMappedRule("loggedOut", phone, Adherence.Out).WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("loggedOut", brejk, Adherence.In)
				.WithMappedRule("phone", phone, Adherence.In)
				.WithMappedRule("phone", brejk, Adherence.Out).WithAlarm(TimeSpan.FromMinutes(5))
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 10:10");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>().Single();
			@event.Timestamp.Should().Be("2018-05-30 10:10".Utc());
			@event.ShiftStart.Should().Be("2018-05-30 08:00".Utc());
		}

		[Test]
		[Ignore("Maybe implement later")]
		public void ShouldPublishWhenArrivingLateAfterFirstBreak()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithAssignment(personId, "2018-05-30")
				.WithActivity(phone)
				.WithAssignedActivity("2018-05-30 8:00", "2018-05-30 17:00")
				.WithActivity(brejk)
				.WithAssignedActivity("2018-05-30 10:00", "2018-05-30 10:15")
				.WithMappedRule("loggedOut", phone, Adherence.Out).WithAlarm(TimeSpan.FromMinutes(5))
				.WithMappedRule("loggedOut", brejk, Adherence.In)
				.WithMappedRule("phone", phone, Adherence.In)
				.WithMappedRule("phone", brejk, Adherence.Out).WithAlarm(TimeSpan.FromMinutes(5))
				;
			Now.Is("2018-05-29 17:00");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedOut"
			});
			Now.Is("2018-05-30 10:20");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonArrivalAfterLateForWorkEvent>().Single();
			@event.Timestamp.Should().Be("2018-05-30 10:20".Utc());
			@event.ShiftStart.Should().Be("2018-05-30 08:00".Utc());
		}

		[Test]
		[Ignore("Per memory notes from friday")]
		//Is there a way to detect if I as an agent have done any state change before start of shift
		//that indicates I am actually at work? Is loggedout the only thing I can work with 
		public void Should_Publish_StartLateForWork_When_Starting_First_Activity_Of_Shift_And_LoggedOut()
		{
			
		}
		
		
		[Test]
		[Ignore("Per memory notes from friday")]
		//The share triggering of a statechange that is not loggedout state indicates the presence of the agent hence I am back in the office and ending late for work.
		//What about threshold here? Should we have a static threshold of ca 4 min to avoid mixing in alarm?
		public void Should_Publish_PersonArrivalAfterLateForWork_When_InLateForWork_And_StateChange()
		{
			
		}

		
		
		
		
		[Test]
		[Ignore("Maybe implement later")]
		public void ShouldWhatShouldBeDoneReallyIfTheFirstActivityIsAllNeutral()
		{
		}

		[Test]
		[Ignore("Maybe implement later")]
		public void ShouldWhatShouldBeDoneReallyIfScheduleIsChangedAndTheShiftStartIsChanged()
		{
		}

		[Test]
		[Ignore("Maybe implement later")]
		public void ShouldWhatShouldBeDoneReallyIfScheduleIsRemoved()
		{
		}
	}
}