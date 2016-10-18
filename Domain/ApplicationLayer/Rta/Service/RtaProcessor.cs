using System;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Collection;

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
		private readonly IAgentStateReadModelUpdater _agentStateReadModelUpdater;
		private readonly IAgentStateTracer _agentStateTracer;

		public RtaProcessor(
			ShiftEventPublisher shiftEventPublisher,
			ActivityEventPublisher activityEventPublisher,
			StateEventPublisher stateEventPublisher,
			AdherenceEventPublisher adherenceEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher eventPublisher,
			IAgentStateReadModelUpdater agentStateReadModelUpdater,
			IAgentStateTracer agentStateTracer)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_adherenceEventPublisher = adherenceEventPublisher;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
			_agentStateTracer = agentStateTracer;
		}

		[LogInfo]
		public virtual void Process(Context context)
		{
			using (var trace = _agentStateTracer.Trace(context))
			{
				try
				{
					if (!context.ShouldProcessState())
					{
						trace.ProcessSkipped();
						return;
					}

					var eventCollector = new EventCollector(_eventPublisher);

					context.UpdateAgentState();
					trace.AgentStateUpdated();

					using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
					{
						_shiftEventPublisher.Publish(context);
						_activityEventPublisher.Publish(context);
						_stateEventPublisher.Publish(context);

						context.Adherence.AdherenceChanges()
							.ForEach(x => _adherenceEventPublisher.Publish(context, x.Time, x.Adherence));
					}

					var events = eventCollector.Publish();
					trace.EventsPublished(events);

					_agentStateReadModelUpdater.Update(context, events);
				}
				finally
				{
					trace.Error();
				}

			}

		}
	}

}
