using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
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
	public class PersonActivityStartEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishEvent()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:00");
			
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.PersonId.Should().Be(personId);
		}

		[Test]
		public void ShouldPublishIfNextActivityHasStarted()
		{
			var personId = Guid.NewGuid();
			var activityId1 = Guid.NewGuid();
			var activityId2 = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId1, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithSchedule(personId, activityId2, "2014-10-20 10:15", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:00");

			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:05");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);
			Now.Is("2014-10-20 10:15");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var events = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>();
			events.Should().Have.Count.EqualTo(2);
		}

		[Test]
		public void ShouldPublishWithActivityInfo()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00");
			Now.Is("2014-10-20 10:02");

			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.StartTime.Should().Be("2014-10-20 10:00".Utc());
			@event.Name.Should().Be("phone");
		}
		
		[Test]
		public void ShouldPublishWithInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", phone, 0);
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Now.Is("2014-10-20 10:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.In);
		}

		[Test]
		public void ShouldPublishWithOutAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, activityId, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("statecode", activityId, 1);
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});
			Now.Is("2014-10-20 10:02");
			Target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Out);
		}
		
		[Test]
		[Toggle(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldPublishWithAdherenceBasedOnPreviousStateAndCurrentActivity()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "phone", "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("admin", phone, 1, Adherence.Neutral)
				.WithAlarm("phone", phone, 0, Adherence.In);
			Now.Is("2014-10-20 09:50");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Now.Is("2014-10-20 10:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single();
			@event.Adherence.Should().Be(EventAdherence.Neutral);
		}

		[Test]
		[ToggleOff(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldNotSetAdhernce()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-13 08:00", "2015-03-13 09:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-13 08:00");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single()
				.Adherence.Should().Be(EventAdherence.In);
		}
	}
}