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
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class EventsStreamTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public Domain.ApplicationLayer.Rta.Service.Rta target;

		[Test]
		public void ShouldPublishAdherenceEventsForBothCausesInASingleTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithSchedule(personId, brejk, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithRule("phone", phone, 0)
				.WithRule("phone", brejk, 1)
				.WithRule("break", brejk, 0)
				.WithRule("break", phone, 1);
			now.Is("2014-10-20 9:00");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			publisher.Clear();

			now.Is("2014-10-20 10:02");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 10:00".Utc());
			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().Adherence.Should().Be(EventAdherence.Out);
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:00".Utc());
			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Adherence.Should().Be(EventAdherence.In);
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
		}

		[Test]
		public void ShouldPublishActivityAndAdherenceEventsFromLastStateChangeWhenRewritingHistory()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule("admin", phone, 1)
				.WithRule("admin", admin, 0);
			now.Is("2014-10-20 9:15");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			publisher.Clear();

			now.Is("2014-10-20 9:30");
			Database.ClearSchedule(personId);
			Database.WithSchedule(personId, admin, "2014-10-20 9:00", "2014-10-20 10:00");
			target.ReloadAndCheckForActivityChanges(Database.TenantName(), personId);

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:15".Utc());
			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().Adherence.Should().Be(EventAdherence.In);
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:15".Utc());
		}

	}
}