using System;
using System.Linq;
using NUnit.Framework;
using SharpTestsEx;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ApplicationLayer.Rta
{
	[TestFixture]
	public class AgentStateChangedInAdherenceCommandHandlerTest
	{
		[Test]
		public void ShouldPublishPersonInAdherenceEventWhenNoStaffingEffect()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.PersonId.Should().Be(state.PersonId);
		}

		[Test]
		public void ShouldNotPublishEventIfStillInAdherence()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);
			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0
			};

			target.Invoke(state);
			target.Invoke(state);

			publisher.PublishedEvents.Should().Have.Count.EqualTo(1);
		}

		[Test]
		public void ShouldPublishTheTimeWhenPersonInAdherence()
		{
			var publisher = new FakeEventsPublisher();
			var target = new AgentStateChangedCommandHandler(publisher);

			var state = new ActualAgentState
			{
				PersonId = Guid.NewGuid(),
				StaffingEffect = 0,
				Timestamp = DateTime.Now
			};

			target.Invoke(state);

			var @event = publisher.PublishedEvents.Single() as PersonInAdherenceEvent;
			@event.Timestamp.Should().Be(state.Timestamp);
		}

	}
}