namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessor
	{
		private readonly IScheduleLoader _scheduleLoader;
		private readonly IStateMapper _stateMapper;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;

		public RtaProcessor(
			IScheduleLoader scheduleLoader,
			IStateMapper stateMapper,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher
			)
		{
			_scheduleLoader = scheduleLoader;
			_stateMapper = stateMapper;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
		}

		public void Process(
			RtaProcessContext context
			)
		{
			if (context.Person == null)
				return;
			var person = context.Person;
			var input = context.Input;

			var scheduleInfo = new ScheduleInfo(_scheduleLoader, context.Person.PersonId, context.CurrentTime);
			var agentStateInfo = new AgentStateInfo(() => context.PreviousState(scheduleInfo), () => context.CurrentState(scheduleInfo));
			var adherenceInfo = new AdherenceInfo(input, person, agentStateInfo, scheduleInfo, _stateMapper);
			var info = new StateInfo(person, agentStateInfo, scheduleInfo, adherenceInfo);
			
			context.AgentStateReadModelUpdater.Update(info);
			context.MessageSender.Send(info);
			context.AdherenceAggregator.Aggregate(info);
			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}
}