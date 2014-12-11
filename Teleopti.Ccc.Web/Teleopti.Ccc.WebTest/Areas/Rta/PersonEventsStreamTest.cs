using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;

namespace Teleopti.Ccc.WebTest.Areas.Rta
{
	[TestFixture]
	public class PersonEventsStreamTest
	{
		[Test]
		public void ShouldPublishAdherenceEventsForBothCausesInASingleTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var brejk = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc())
				.WithSchedule(personId, brejk, "2014-10-20 10:00".Utc(), "2014-10-20 10:15".Utc())
				.WithAlarm("phone", phone, 0)
				.WithAlarm("phone", brejk, 1)
				.WithAlarm("break", brejk, 0)
				.WithAlarm("break", phone, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);
			now.Mutate("2014-10-20 9:00");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
				Timestamp = "2014-10-20 9:00".Utc()
			});
			publisher.PublishedEvents.Clear();

			now.Mutate("2014-10-20 10:02");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "break",
				Timestamp = "2014-10-20 10:02".Utc()
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
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc())
				.WithAlarm("admin", phone, 1)
				.WithAlarm("admin", admin, 0)
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow();
			var target = new RtaForTest(database, now, publisher);
			now.Mutate("2014-10-20 9:15");
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin",
				Timestamp = "2014-10-20 9:15".Utc()
			});
			publisher.PublishedEvents.Clear();

			now.Mutate("2014-10-20 9:30");
			database.ClearSchedule(personId);
			database.WithSchedule(personId, admin, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc());
			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 9:31".Utc());

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:15".Utc());
			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().InAdherence.Should().Be.True();
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:15".Utc());
		}

		[Test]
		public void ShouldSwitchToNextActivityOnceEvenThoughTimeDifferenceBetweenSourceStreams()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithBusinessUnit(businessUnitId)
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc())
				.WithSchedule(personId, admin, "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventPublisher();
			var now = new MutableNow("2014-10-20 9:00");
			var target = new RtaForTest(database, now, publisher);
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone",
				Timestamp = "2014-10-20 9:00".Utc()
			});
			publisher.PublishedEvents.Clear();

			now.Mutate("2014-10-20 10:00");
			target.CheckForActivityChange(personId, businessUnitId, "2014-10-20 10:00".Utc());
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "admin",
				Timestamp = "2014-10-20 9:59".Utc()
			});

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Should().Have.Count.EqualTo(1);
		}
	}
}