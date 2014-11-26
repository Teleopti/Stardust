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
				StateCode = "statecode1",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var state2 = new ExternalUserStateForTest
			{
				UserCode = "usercode2",
				StateCode = "statecode2",
				Timestamp = new DateTime(2014, 10, 20, 9, 0, 0, DateTimeKind.Utc)
			};
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.WithDefaultsFromState(state1)
				.WithUser("usercode1", personId1)
				.WithUser("usercode2", personId2)
				.WithSchedule(personId1, activityId, state1.Timestamp.AddHours(-1), state1.Timestamp.AddHours(1))
				.WithSchedule(personId2, activityId, state2.Timestamp.AddHours(-1), state2.Timestamp.AddHours(1))
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 1)
				.Make();
			var publisher = new FakeEventPublisher();
			var target = new RtaForTest(database, new ThisIsNow(state1.Timestamp), publisher);

			target.SaveState(state1);
			target.SaveState(state2);
			target.SaveState(state1);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Where(x => x.PersonId == personId1).Should().Have.Count.GreaterThan(0);
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Where(x => x.PersonId == personId2).Should().Have.Count.GreaterThan(0);
		}

	}
}