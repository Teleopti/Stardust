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
	public class EventOrderTest
	{
		[Test]
		public void ShouldPublishShiftStartBeforeActivityStart()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonShiftStartEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishActivityStartBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishStateChangeBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00".Utc(), "2014-10-20 11:00".Utc())
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode",
				Timestamp = "2014-10-20 10:00".Utc()
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}
	}
}