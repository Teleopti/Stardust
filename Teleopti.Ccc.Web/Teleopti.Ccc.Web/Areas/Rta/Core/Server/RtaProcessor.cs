using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaProcessor
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IAlarmFinder _alarmFinder;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly INow _now;

		public RtaProcessor(IDatabaseReader databaseReader,
			IAlarmFinder alarmFinder,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			INow now
			)
		{
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_now = now;
		}

		public void Process(
			RtaProcessContext context
			)
		{
			if (context.Person == null)
				return;
			var person = context.Person;
			var input = context.Input;

			var scheduleInfo = new ScheduleInfo(_databaseReader, context.Person.PersonId, context.CurrentTime);
			var agentStateInfo = new AgentStateInfo(() => context.PreviousState(scheduleInfo), () => context.CurrentState(scheduleInfo));
			var adherenceInfo = new AdherenceInfo(input, person, agentStateInfo, scheduleInfo, _alarmFinder);
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