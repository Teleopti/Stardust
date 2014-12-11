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
	public class EventsTest
	{
		[Test]
		public void ShouldPublishEventsForEachPerson()
		{
			var state1 = new ExternalUserStateForTest
			{
				UserCode = "usercode1",
				StateCode = "statecode1"
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode2",
				StateCode = "statecode2"
			};
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode1", personId1)
				.WithUser("usercode2", personId2)
				.WithSchedule(personId1, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow("2014-10-20 9:00"), publisher);

			target.SaveState(state1);
			target.SaveState(state2);
			target.SaveState(state1);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Where(x => x.PersonId == personId1).Should().Have.Count.GreaterThan(0);
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Where(x => x.PersonId == personId2).Should().Have.Count.GreaterThan(0);
		}

	}
}