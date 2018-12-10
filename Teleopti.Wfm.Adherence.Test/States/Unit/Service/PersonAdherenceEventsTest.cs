using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.TestCommon;
using Teleopti.Ccc.TestCommon.FakeRepositories;
using Teleopti.Ccc.TestCommon.FakeRepositories.Rta;
using Teleopti.Wfm.Adherence.Domain.Events;
using Teleopti.Wfm.Adherence.States;

namespace Teleopti.Wfm.Adherence.Test.States.Unit.Service
{
	[TestFixture]
	[RtaTest]
	public class PersonAdherenceEventsTest
	{
		public FakeDatabase Database;
		public MutableNow Now;
		public FakeEventPublisher Publisher;
		public Rta Target ;
		public RtaTestAttribute Context;

		[Test]
		public void ShouldPublishEventsForEachPerson()
		{
			Now.Is("2014-10-20 9:00");
			var personId1 = Guid.NewGuid();
			var personId2 = Guid.NewGuid();
			var activityId = Guid.NewGuid();
			Database
				.WithAgent("usercode1", personId1)
				.WithAgent("usercode2", personId2)
				.WithSchedule(personId1, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithSchedule(personId2, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithMappedRule("statecode1", activityId, 0)
				.WithMappedRule("statecode2", activityId, 1)
				;

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				StateCode = "statecode1"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode2",
				StateCode = "statecode2"
			});
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode1",
				StateCode = "statecode1"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Where(x => x.PersonId == personId1).Should().Have.Count.GreaterThan(0);
			Publisher.PublishedEvents.OfType<PersonOutOfAdherenceEvent>().Where(x => x.PersonId == personId2).Should().Have.Count.GreaterThan(0);
		}

		[Test]
		public void ShouldNotSendDuplicateAdherenceEventsAfterRestart()
		{
			Now.Is("2014-10-20 9:00");
			var activityId = Guid.NewGuid();
			var personId = Guid.NewGuid();
			Database
				.WithAgent("usercode", personId)
				.WithSchedule(personId, activityId, "2014-10-20 8:00", "2014-10-20 10:00")
				.WithMappedRule("statecode1", activityId, 0)
				.WithMappedRule("statecode2", activityId, 0);

			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode1"
			});
			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(1);
			Publisher.Clear();
			Context.SimulateRestart();
			Target.ProcessState(new StateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode2"
			});

			Publisher.PublishedEvents.OfType<PersonInAdherenceEvent>().Should().Have.Count.EqualTo(0);
		}

	}
}