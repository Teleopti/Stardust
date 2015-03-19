using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;
using Teleopti.Interfaces.Messages;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	public class PersonStateChangedEventTest
	{
		public FakeRtaDatabase database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public FakeCurrentDatasource dataSource;
		public IRta target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishEventOnlyIfStateChanged()
		{
			var personId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithAlarm("stateone", Guid.NewGuid())
				.WithAlarm("statetwo", Guid.NewGuid());

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "stateone"
			});
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statetwo"
			});

			var events = publisher.PublishedEvents.OfType<PersonStateChangedEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithSystemTime()
		{
			var personId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId);
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.Timestamp.Should().Be("2014-10-20 10:00".Utc());
		}

		[Test]
		public void ShouldPublishWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId);
			dataSource.FakeName("datasource");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = (ILogOnInfo)publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, 0);
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherence.Should().Be(true);
		}

		[Test]
		public void ShouldPublishWithOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("break", phone, 1);
			now.Is("2014-10-20 10:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InAdherence.Should().Be(false);
		}

		[Test]
		public void ShouldPublishInAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 0);
			now.Is("2014-10-20 11:05");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InOrNeutralAdherenceWithPreviousActivity.Should().Be(true);
		}

		[Test]
		public void ShouldPublishOutOfAdherenceForPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 1);
			now.Is("2014-10-20 11:05");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single();
			@event.InOrNeutralAdherenceWithPreviousActivity.Should().Be(false);
		}


		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldSetAdhernce()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-13 08:00", "2015-03-13 09:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			now.Is("2015-03-13 08:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.Adherence.Should().Be(AdherenceState.Neutral);
		}

		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldSetNeutralAdhernceWhenAlarmIsNotConfigured()
		{
			var personId = Guid.NewGuid();
			database
				.WithUser("usercode", personId);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.Adherence.Should().Be(AdherenceState.Neutral);
		}

		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldSetInAdhernceWithPreviousActivity()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, -1, Adherence.Neutral);
			now.Is("2014-10-20 11:05");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single()
				.InOrNeutralAdherenceWithPreviousActivity.Should().Be(true);
		}

		[Test]
		[ToggleOff(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldNotSetAdhernceWhenToggleIsOff()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-13 08:00", "2015-03-13 09:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			now.Is("2015-03-13 08:00");

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Adherence.Should().Be(null);
		}

	}
}