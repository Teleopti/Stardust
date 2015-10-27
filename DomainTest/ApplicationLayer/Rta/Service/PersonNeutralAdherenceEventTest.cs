using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class PersonNeutralAdherenceEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		[ToggleOff(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldNotUseAdherencePropertyIfToggleOff()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(0);
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublish()
		{
			var personId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldNotPublishWhenInAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("phone", phone, 0, Adherence.In);
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Be.Empty();
		}

		[Test]
		public void ShouldNotPublishWhenOutOfAdherence()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("break", phone, 0, Adherence.Out);
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Be.Empty();
		}
		
		[Test]
		public void ShouldNotPublishIfStillNeutralAdherence()
		{
			var personId = Guid.NewGuid();
			var admin1 = Guid.NewGuid();
			var admin2 = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, admin1, "2015-03-10 8:00", "2015-03-10 9:00")
				.WithSchedule(personId, admin2, "2015-03-10 9:00", "2015-03-10 11:00")
				.WithAlarm("admin1", admin1, 0, Adherence.Neutral)
				.WithAlarm("admin1", admin2, 0, Adherence.Neutral)
				.WithAlarm("admin2", admin1, 0, Adherence.Neutral)
				.WithAlarm("admin2", admin2, 0, Adherence.Neutral)
				;

			Now.Is("2015-03-10 8:05");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin1"
			});
			Now.Is("2015-03-10 9:05");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin2"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}


		[Test]
		public void ShouldPublishWhenAlarmMappingIsMissing()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2015-03-19 8:00", "2015-03-19 9:00")
				.WithAlarm("phone", phone, 0, Adherence.In)
				;
			Now.Is("2015-03-19 8:01");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "someOtherCode"
			});

			Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId, teamId, siteId)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.Timestamp.Should().Be("2015-03-10 8:30".Utc());
			@event.TeamId.Should().Be(teamId);
			@event.SiteId.Should().Be(siteId);
		}


		[Test]
		public void ShouldPublishEventWithBusinessUnitId()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId, null, null)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});

			var @event = Publisher.PublishedEvents.OfType<PersonNeutralAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
		}
	}
}