using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Ccc.TestCommon.IoC;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta.Service
{
	[TestFixture]
	[RtaTest]
	[Toggle(Toggles.RTA_NewEventHangfireRTA_34333)]
	public class PersonAdherenceEventsTest
	{
		public FakeRtaDatabase database;
		public MutableNow now;
		public FakeEventPublisher publisher;
		public Domain.ApplicationLayer.Rta.Service.Rta target ;
		public RtaTestAttribute context;

		[Test]
		public void ShouldPublishEventsForEachPerson()
		{
			now.Is("2014-10-20 9:00");
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
			database.WithDefaultsFromState(state1)
				.WithUser("usercode1", personId1)
				.WithUser("usercode2", personId2)
				.WithSchedule(personId1, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 1)
				;

			target.SaveState(state1);
			target.SaveState(state2);
			target.SaveState(state1);

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Where(x => x.PersonId == personId1).Should().Have.Count.GreaterThan(0);
			publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Where(x => x.PersonId == personId2).Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotSendDuplicateAdherenceEventsAfterRestart()
		{
			now.Is("2014-10-20 9:00");
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			database
				.WithUser("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithAlarm("statecode1", activityId, 0)
				.WithAlarm("statecode2", activityId, 0);

			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			});
			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
			publisher.Clear();
			context.SimulateRestart();
			target.SaveState(new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			});

			publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(0);
		}
	}
}