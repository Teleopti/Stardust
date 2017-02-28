using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class EventsOrderTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishShiftStartBeforeActivityStart()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
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
				.WithRule("phone", phone, 0);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
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
				.WithRule("phone", activityId, 1);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
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
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("phone", null, 1, Adherence.Out)
				.WithRule("loggedout", null, 0, Adherence.In)
				.WithRule("loggedout", phone, -1, Adherence.Out)
				;
			Now.Is("2014-10-20 7:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "loggedout"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 8:05");
			Target.SaveState(new StateForTest
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
				.WithRule("phone", phone, 0, Adherence.In)
				.WithRule("phone", null, 1, Adherence.Neutral)
				.WithRule("loggedout", null, 0, Adherence.In)
				.WithRule("loggedout", phone, -1, Adherence.Out)
				;
			Now.Is("2014-10-20 16:55");
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 17:05");
			Target.SaveState(new StateForTest
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