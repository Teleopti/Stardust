using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessor
	{
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly AgentStateReadModelUpdater _agentStateReadModelUpdater;

		public RtaProcessor(
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher eventPublisher,
			AgentStateReadModelUpdater agentStateReadModelUpdater)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
			_agentStateReadModelUpdater = agentStateReadModelUpdater;
		}

		[InfoLog]
		public virtual void Process(Context context)
		{
			if (!context.ShouldProcessState())
				return;
			
			var eventCollector = new EventCollector(_eventPublisher);
			
			context.UpdateAgentState();

			using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
			{
				_shiftEventPublisher.Publish(context);
				_activityEventPublisher.Publish(context);
				_stateEventPublisher.Publish(context);
			}

			eventCollector.PublishTransitions();

			_agentStateReadModelUpdater.Update(context.MakeAgentStateReadModel());
		}
	}
}