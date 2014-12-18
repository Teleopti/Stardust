using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.IoC;
using Teleopti.Ccc.Web.Areas.Rta;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_SeeAdherenceDetailsForOneAgent_31285)]
	[Toggle(Toggles.RTA_SeePercentageAdherenceForOneAgent_30783)]
	public class PersonEventsStreamTest
	{
		public FakeRtaDatabase database;
		public FakeEventPublisher publisher;
		public MutableNow now;
		public IRta target;

		[Test]
		public void ShouldPublishAdherenceEventsForBothCausesInASingleTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithSchedule(personId, brejk, "2014-10-20 10:00", "2014-10-20 10:15")
				.WithAlarm("phone", phone, 0)
				.WithAlarm("phone", brejk, 1)
				.WithAlarm("break", brejk, 0)
				.WithAlarm("break", phone, 1);
			now.Is("2014-10-20 9:00");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});
			publisher.PublishedEvents.Clear();

			now.Is("2014-10-20 10:02");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break"
			});

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 10:00".Utc());
			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().InAdherence.Should().Be.False();
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:00".Utc());
			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().InAdherence.Should().Be.True();
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
		}

		[Test]
		public void ShouldPublishActivityAndAdherenceEventsFromLastStateChangeWhenRewritingHistory()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			database
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00", "2014-10-20 10:00")
				.WithAlarm("admin", phone, 1)
				.WithAlarm("admin", admin, 0);
			now.Is("2014-10-20 9:15");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin"
			});
			publisher.PublishedEvents.Clear();

			now.Is("2014-10-20 9:30");
			database.ClearSchedule(personId);
			database.WithSchedule(personId, admin, "2014-10-20 9:00", "2014-10-20 10:00");
			target.CheckForActivityChange(personId, businessUnitId);

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:15".Utc());
			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().InAdherence.Should().Be.True();
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:15".Utc());
		}

	}
}