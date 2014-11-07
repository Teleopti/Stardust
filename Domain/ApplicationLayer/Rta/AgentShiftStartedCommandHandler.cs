using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AgentShiftStartedCommandHandler : IActualAgentStateHasBeenSent
	{
		private readonly IEventPublisher _eventPublisher;

		public AgentShiftStartedCommandHandler(IEventPublisher eventPublisher)
		{
			_eventPublisher = eventPublisher;
		}

		public void Invoke(IActualAgentState actualAgentState)
		{
			if (actualAgentState.ScheduledId != Guid.Empty)
			{
				_eventPublisher.Publish(new PersonShiftStartEvent
				{
					PersonId = actualAgentState.PersonId
				});
			}
		}
	}
}