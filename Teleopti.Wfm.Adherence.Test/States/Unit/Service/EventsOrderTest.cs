using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.Domain.Service;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class EventsOrderTest
	{
		public FakeDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Rta Target;

		[Test]
		public void ShouldPublishShiftStartBeforeActivityStart()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var before = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single());
			var after = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishActivityStartBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("phone", phone, 0);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var before = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			var after = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishStateChangeBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithMappedRule("phone", activityId, 1);
			Now.Is("2014-10-20 10:00");

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var before = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var after = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishAdherenceEventsAfterItsTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 17:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("phone", null, 1, Adherence.Configuration.Adherence.Out)
				.WithMappedRule("loggedout", null, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("loggedout", phone, -1, Adherence.Configuration.Adherence.Out)
				;
			Now.Is("2014-10-20 7:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedout"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 8:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var event0 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single());
			var event1 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			var event2 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single());
			var event3 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var event4 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			event0.Should().Be.LessThan(event1);
			event1.Should().Be.LessThan(event2);
			event2.Should().Be.LessThan(event3);
			event3.Should().Be.LessThan(event4);
		}

		[Test]
		public void ShouldPublishAdherenceEventsAfterItsTrigger2()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 8:00", "2014-10-20 17:00")
				.WithMappedRule("phone", phone, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("phone", null, 1, Adherence.Configuration.Adherence.Neutral)
				.WithMappedRule("loggedout", null, 0, Adherence.Configuration.Adherence.In)
				.WithMappedRule("loggedout", phone, -1, Adherence.Configuration.Adherence.Out)
				;
			Now.Is("2014-10-20 16:55");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 17:05");
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedout"
			});

			var event0 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonShiftEndEvent>().Single());
			var event1 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single());
			var event2 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var event3 = Publisher.PublishedEvents.IndexOf(Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			event0.Should().Be.LessThan(event1);
			event1.Should().Be.LessThan(event2);
			event2.Should().Be.LessThan(event3);
		}

	}
}