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
		[Test, Ignore]
		public void ShouldPublishAdherenceEventsForBothCausesInASingleTrigger()
		{
			var personId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var breakk = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, phone, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc())
				.WithSchedule(personId, breakk, "2014-10-20 10:00".Utc(), "2014-10-20 10:15".Utc())
				.WithAlarm("phone", phone, 0)
				.WithAlarm("break", phone, 1)
				.WithAlarm("break", breakk, 0)
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
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:00".Utc());
			publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 10:02".Utc());
		}

		[Test, Ignore]
		public void ShouldPublishActivityAndAdherenceEventsFromNowWhenChangingThePast()
		{
			var personId = Guid.NewGuid();
			var businessUnitId = Guid.NewGuid();
			var phone = Guid.NewGuid();
			var admin = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId, businessUnitId)
				.WithSchedule(personId, phone, "2014-10-20 9:00".Utc(), "2014-10-20 10:00".Utc())
				.WithAlarm("phone", phone, 0)
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

			publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single().StartTime.Should().Be("2014-10-20 9:30".Utc());
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single().Timestamp.Should().Be("2014-10-20 9:30".Utc());
		}
	}
}