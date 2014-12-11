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
	public class PersonEventsOrderTest
	{
		[Test]
		public void ShouldPublishShiftStartBeforeActivityStart()
		{
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
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
				.WithSchedule(personId, Guid.NewGuid(), "2014-10-20 10:00", "2014-10-20 11:00")
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonActivityStartEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}

		[Test]
		public void ShouldPublishStateChangeBeforeAdherence()
		{
			var personId = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(new ExternalUserStateForTest())
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 10:00", "2014-10-20 11:00")
				.WithAlarm("phone", activityId, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 10:00"), publisher);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "phone"
			});

			var before = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonStateChangedEvent>().Single());
			var after = publisher.PublishedEvents.IndexOf(publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Single());
			before.Should().Be.LessThan(after);
		}
	}
}