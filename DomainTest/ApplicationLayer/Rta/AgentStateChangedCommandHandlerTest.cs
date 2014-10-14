using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AgentStateChangedCommandHandlerTest
	{
		[Test]
		public void ShouldPublishEventsForEachPerson()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state1 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0,
				ScheduledId = Guid.NewGuid()
			};
			var state2 = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 1,
				ScheduledId = Guid.NewGuid()
			};

			target.Invoke(state1);
			target.Invoke(state2);
			target.Invoke(state1);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(2);
			var event1 = publisher.PublishedEvents.ElementAt(0) as PersonInAdherenceEvent;
			event1.PersonId.Should().Be(state1.PersonId);
			var event2 = publisher.PublishedEvents.ElementAt(1) as PersonOutOfAdherenceEvent;
			event2.PersonId.Should().Be(state2.PersonId);
		}

		[Test]
		public void ShouldNotPublishEventsForPersonWithNoScheduledActivity()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0,
				ScheduledId = Guid.Empty
			};
			
			target.Invoke(state);
			
			publisher.PublishedEvents.Should().Have.Count.EqualTo(0);
		}

	}

}