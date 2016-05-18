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
	public class EventsStreamTest
	{
		public FakeRtaDatabase Database;
		public FakeEventPublisher Publisher;
		public MutableNow Now;
		public Domain.ApplicationLayer.Rta.Service.Rta Target;

		[Test]
		public void ShouldPublishAdherenceEventsForBothCausesInASingleTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithSchedule(personId, brejk, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithRule("phone", phone, 0)
				.WithRule("phone", brejk, 1)
				.WithRule("break", brejk, 0)
				.WithRule("break", phone, 1);
			Now.Is("2014-10-20 9:00");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 10:02");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().Adherence.Should().Be(EventAdherence.Out);
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:00".Utc());
			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
			Publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Adherence.Should().Be(EventAdherence.In);
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
		}

		[Test]
		public void ShouldPublishActivityAndAdherenceEventsFromLastStateChangeWhenRewritingHistory()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			Database
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithRule("admin", phone, 1)
				.WithRule("admin", admin, 0);
			Now.Is("2014-10-20 9:15");
			Target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			Publisher.Clear();

			Now.Is("2014-10-20 9:30");
			Database.ClearSchedule(personId);
			Database.WithSchedule(personId, admin, "2014-10-20 9:00", "2014-10-20 10:00");
			Target.CheckForActivityChanges(Database.TenantName(), personId);

			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:15".Utc());
			Publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().Adherence.Should().Be(EventAdherence.In);
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:15".Utc());
		}

	}
}