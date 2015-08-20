using System;
using System.Collections.Concurrent;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessor
	{
		private readonly IScheduleLoader _scheduleLoader;
		private readonly IStateMapper _stateMapper;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly ConcurrentDictionary<Guid, object> personLocks = new ConcurrentDictionary<Guid, object>();

		public RtaProcessor(
			IScheduleLoader scheduleLoader,
			IStateMapper stateMapper,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			IAppliedAdherence appliedAdherence
			)
		{
			_scheduleLoader = scheduleLoader;
			_stateMapper = stateMapper;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_appliedAdherence = appliedAdherence;
		}

		public void Process(
			RtaProcessContext context
			)
		{
			if (context.Person == null)
				return;

			lock (personLocks.GetOrAdd(context.Person.PersonId, g => new object()))
			{
				var person = context.Person;

				StateInfo info = null;
				info = new StateInfo(
					person,
					() => context.PreviousState(info),
					() => context.CurrentState(info),
					context.Input,
					context.CurrentTime,
					_stateMapper,
					_scheduleLoader,
					_appliedAdherence);

				context.AgentStateReadModelUpdater.Update(info);
				context.MessageSender.Send(info);
				context.AdherenceAggregator.Aggregate(info);
				_shiftEventPublisher.Publish(info);
				_activityEventPublisher.Publish(info);
				_stateEventPublisher.Publish(info);	
			}
		}
	}
}