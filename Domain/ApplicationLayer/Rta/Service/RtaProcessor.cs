using System;
using System.Collections.Concurrent;
using Teleopti.Ccc.Domain.Aop;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessor
	{
		private readonly IDatabaseLoader _databaseLoader;
		private readonly IStateMapper _stateMapper;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly IAppliedAdherence _appliedAdherence;
		private readonly IEventPublisherScope _eventPublisherScope;
		private readonly ICurrentEventPublisher _eventPublisher;
		private readonly ConcurrentDictionary<Guid, object> personLocks = new ConcurrentDictionary<Guid, object>();
		private readonly IAppliedAlarm _appliedAlarm;

		public RtaProcessor(
			IDatabaseLoader databaseLoader,
			IStateMapper stateMapper,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			IAppliedAdherence appliedAdherence,
			IEventPublisherScope eventPublisherScope,
			ICurrentEventPublisher eventPublisher, 
			IAppliedAlarm appliedAlarm)
		{
			_databaseLoader = databaseLoader;
			_stateMapper = stateMapper;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_appliedAdherence = appliedAdherence;
			_eventPublisherScope = eventPublisherScope;
			_eventPublisher = eventPublisher;
			_appliedAlarm = appliedAlarm;
		}

		[InfoLog]
		public virtual void Process(StateContext context)
		{
			lock (personLocks.GetOrAdd(context.PersonId, g => new object()))
			{
				var eventCollector = new EventCollector(_eventPublisher);

				var info = new StateInfo(
					context,
					_stateMapper,
					_databaseLoader,
					_appliedAdherence,
					_appliedAlarm);

				context.AgentStateReadModelUpdater.Update(info);

				using (_eventPublisherScope.OnThisThreadPublishTo(eventCollector))
				{
					_shiftEventPublisher.Publish(info);
					_activityEventPublisher.Publish(info);
					_stateEventPublisher.Publish(info);
				}

				eventCollector.PublishTransitions();
			}
		}
	}
}