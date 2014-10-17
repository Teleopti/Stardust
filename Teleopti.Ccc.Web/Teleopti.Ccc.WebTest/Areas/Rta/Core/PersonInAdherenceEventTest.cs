using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;

namespace Teleopti.Ccc.WebTest.Areas.Rta.Core
{
	[TestFixture]
	public class PersonInAdherenceEventTest
	{
		[Test, Ignore]
		public void ShouldPublishPersonInAdherenceEvent() // (...WhenNoStaffingEffect)
		{
			var state = new ExternalUserStateForTest
			{
				UserCode = "usercode",
				StateCode = "statecode"
			};
			var database = new FakeRtaDatabase();
			var personId = Guid.NewGuid();
			database.AddTestData(state.SourceId, "usercode", personId, Guid.NewGuid());
			var publisher = new FakeEventsPublisher();
			var target = new TeleoptiRtaServiceForTest(state, database, publisher);

			//var state = new ActualAgentState
			//{
			//	PersonId = Guid.NewGuid(),
			//	StaffingEffect = 0
			//};

			//target.Invoke(state);

			target.SaveExternalUserState(state);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.PersonId.Should().Be(personId);
		}
	}
}