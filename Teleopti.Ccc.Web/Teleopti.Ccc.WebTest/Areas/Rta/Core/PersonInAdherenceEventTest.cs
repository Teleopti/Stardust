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
			var personId = Guid.NewGuid();
			var database = new FakeRtaDatabase()
				.AddSource(state.SourceId)
				.AddUser("usercode", personId, Guid.NewGuid())
				.Done();
			var publisher = new FakeEventsPublisher();
			var target = TeleoptiRtaServiceForTest.MakeBasedOnState(state, database, publisher);

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