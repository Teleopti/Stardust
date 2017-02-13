﻿using System.Collections.Generic;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessor
	{
		private readonly ShiftEventPublisher _shiftEventPublisher;
		private readonly ActivityEventPublisher _activityEventPublisher;
		private readonly StateEventPublisher _stateEventPublisher;
		private readonly AdherenceEventPublisher _adherenceEventPublisher;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly ICurrentEventPublisher _eventPublisher;

		public RtaProcessor(
			ShiftEventPublisher shiftEventPublisher,
			ActivityEventPublisher activityEventPublisher,
			StateEventPublisher stateEventPublisher,
			AdherenceEventPublisher adherenceEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher eventPublisher)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
		}

		[LogInfo]
		public virtual IEnumerable<IEvent> Process(Context context)
		{
			var eventCollector = new EventCollector(_eventPublisher);

			context.UpdateAgentState();

			using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
			{
				_shiftEventPublisher.Publish(context);
				_activityEventPublisher.Publish(context);
				_stateEventPublisher.Publish(context);

				context.Adherence.AdherenceChanges()
					.ForEach(x => _adherenceEventPublisher.Publish(context, x.Time, x.Adherence));
			}

			return eventCollector.Publish();
		}
	}

}
