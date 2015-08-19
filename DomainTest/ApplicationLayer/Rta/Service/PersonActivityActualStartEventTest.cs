using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[Ignore]
	public class PersonActivityActualStartEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public FakeCurrentDatasource DataSource;
		public MutableNow Now;
		public IRta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithAlarm("phone", phone, 0, Adherence.In);
			Now.Is("2015-08-19 08:00");
			
			Target.CheckForActivityChange(personId, businessUnitId);
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldNotPublishEventWhenNotStarted()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithAlarm("phone", phone, 0, Adherence.In);
			Now.Is("2015-08-19 08:00");

			Target.CheckForActivityChange(personId, businessUnitId);

			Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>()
				.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldNeverPublishEventWhenNeverStarted()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var lunch = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithSchedule(personId, lunch, "2015-08-19 09:00", "2015-08-19 10:00")
				.WithAlarm("phone", phone, 0, Adherence.In);

			Now.Is("2015-08-19 08:00");
			Target.CheckForActivityChange(personId, businessUnitId);
			Now.Is("2015-08-19 09:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>()
				.Should().Have.Count.EqualTo(0);
		}

		[Test]
		public void ShouldPublishEventWhenInAdherenceAsActivityStarts()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithAlarm("phone", phone, 0, Adherence.In);
			Now.Is("2015-08-19 08:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChange(personId, businessUnitId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>().Single();
			@event.StartTime.Should().Be("2015-08-19 08:00".Utc());
		}

		[Test]
		public void ShouldPublishEventWhenInAdherenceBeforeActivityStarted()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithAlarm("phone", phone, 0, Adherence.In);

			Now.Is("2015-08-19 07:55");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2015-08-19 08:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>().Single();
			@event.StartTime.Should().Be("2015-08-19 07:55".Utc());
		}

		[Test]
		public void ShouldPublishEventWhenInAdherenceAfterActivityStarted()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithAlarm("phone", phone, 0, Adherence.In);

			Now.Is("2015-08-19 08:00");
			Target.CheckForActivityChange(personId, businessUnitId);
			Now.Is("2015-08-19 08:05");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>().Single();
			@event.StartTime.Should().Be("2015-08-19 08:05".Utc());
		}

		[Test]
		public void ShouldPersistActualStartTimeWhenStartedEarlier()
		{
			// ???
		}

		[Test]
		public void ShouldPublishEventWhenAdherenceDoesNotChangeWhenActivityChanges()
		{
			var personId = Guid.NewGuid();
			var phone1 = Guid.NewGuid();
			var phone2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone1, "2015-08-19 08:00", "2015-08-19 09:00")
				.WithSchedule(personId, phone2, "2015-08-19 09:00", "2015-08-19 10:00")
				.WithAlarm("phone", phone1, 0, Adherence.In)
				.WithAlarm("phone", phone2, 0, Adherence.In);
			Now.Is("2015-08-19 08:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.CheckForActivityChange(personId, businessUnitId);
			Publisher.Clear();

			Now.Is("2015-08-19 09:00");
			Target.CheckForActivityChange(personId, businessUnitId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityActualStartEvent>().Single();
			@event.StartTime.Should().Be("2015-08-19 09:00".Utc());
		}

		[Test]
		public void ShouldNotPersistActualStartTimeWhenSecondActivityNeverStarted()
		{
			// ???
		}

		[Test]
		public void BelongsToDate()
		{
			// ???
		}

	}

	public class PersonActivityActualStartEvent : IEvent
	{
		public Guid PersonId { get; set; }
		public DateTime StartTime { get; set; }
		//public DateOnly? BelongsToDate { get; set; }
	}

}