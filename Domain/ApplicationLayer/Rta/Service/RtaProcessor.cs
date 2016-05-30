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

		public RtaProcessor(
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher eventPublisher)
		{
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
		}

		[InfoLog]
		public virtual void Process(Context context)
		{
			if (context.ShouldUpdateReadModel())
				context.UpdateAgentStateReadModel();

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
		}
	}
}