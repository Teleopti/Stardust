using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonStateChangedEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId);

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishEventOnlyIfStateChanged()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithRule("stateone", Guid.NewGuid())
				.WithRule("statetwo", Guid.NewGuid());

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});

			var events = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithSystemTime()
		{
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 10:00".Utc());
		}
		
		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("statecode", activityId, 0);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishWithOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("break", phone, 1);
			Now.Is("2014-10-20 10:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Out);
		}

		[Test]
		public void ShouldPublishInAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("phone", activityId, 0);
			Now.Is("2014-10-20 11:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.AdherenceWithPreviousActivity.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishOutOfAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("phone", activityId, 1);
			Now.Is("2014-10-20 11:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.AdherenceWithPreviousActivity.Should().Be(EventAdherence.Out);
		}

		[Test]
		public void ShouldPublishWithNeutralAdhernce()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-13 08:00", "2015-03-13 09:00")
				.WithRule("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-13 08:00");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.Adherence.Should().Be(EventAdherence.Neutral);
		}
		
		[Test]
		public void ShouldPublishNeutralAdhernceWithPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithRule("phone", activityId, -1, Adherence.Neutral);
			Now.Is("2014-10-20 11:05");

			Target.SaveState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.AdherenceWithPreviousActivity.Should().Be(EventAdherence.Neutral);
		}

	}
}