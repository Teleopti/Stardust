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
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	[Toggle(Toggles.RTA_NeutralAdherence_30930)]
	public class PersonUnknownAdherenceEventTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public FakeCurrentDatasource DataSource;
		public MutableNow Now;
		public IRta Target;
		
		[Test]
		public void ShouldPublishWhenNoAlarm()
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

			Publisher.PublishedEvents.OfType<PersonUnknownAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishWithProperties()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var siteId = Guid.NewGuid();
			var teamId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId, teamId, siteId)
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

			var @event = Publisher.PublishedEvents.OfType<PersonUnknownAdherenceEvent>().Single();
			@event.PersonId.Should().Be(personId);
			@event.Timestamp.Should().Be("2015-03-19 8:01".Utc());
			@event.TeamId.Should().Be(teamId);
			@event.SiteId.Should().Be(siteId);
		}

		[Test]
		public void ShouldPublishEventWithLogOnInfo()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId, businessUnitId, null, null)
				.WithSchedule(personId, admin, "2015-03-10 8:00", "2015-03-10 10:00")
				.WithAlarm("admin", admin, 0, Adherence.Neutral);
			DataSource.FakeName("datasource");
			Now.Is("2015-03-10 8:30");

			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "strangecode"
			});

			var @event = (ILogOnInfo) Publisher.PublishedEvents.OfType<PersonUnknownAdherenceEvent>().Single();
			@event.BusinessUnitId.Should().Be(businessUnitId);
			@event.Datasource.Should().Be("datasource");
		}

		[Test]
		[ToggleOff(Toggles.RTA_NeutralAdherence_30930)]
		public void ShouldNowPublishWhenToggleIsOff()
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

			Publisher.PublishedEvents.OfType<PersonUnknownAdherenceEvent>().Should().Have.Count.EqualTo(0);
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
		}
	}
}