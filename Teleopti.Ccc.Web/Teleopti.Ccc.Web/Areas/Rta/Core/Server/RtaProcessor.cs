using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaProcessor
	{
		private readonly INow _now;
		private readonly IPersonOrganizationProvider _personOrganizationProvider;
		private readonly IDatabaseReader _databaseReader;
		private readonly IAlarmFinder _alarmFinder;
		private readonly AgentStateAssembler _agentStateAssembler;
		private readonly IAgentStateMessageSender _messageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;
		private readonly IActualAgentStateUpdater _actualAgentStateUpdater;

		public RtaProcessor(
			INow now,
			IPersonOrganizationProvider personOrganizationProvider, 
			IDatabaseReader databaseReader,
			IAlarmFinder alarmFinder,
			AgentStateAssembler agentStateAssembler,
			IAgentStateMessageSender messageSender,
			IAdherenceAggregator adherenceAggregator,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher,
			IActualAgentStateUpdater actualAgentStateUpdater
			)
		{
			_now = now;
			_personOrganizationProvider = personOrganizationProvider;
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
			_agentStateAssembler = agentStateAssembler;
			_messageSender = messageSender;
			_adherenceAggregator = adherenceAggregator;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
			_actualAgentStateUpdater = actualAgentStateUpdater;
		}

		public void Process(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId
			)
		{
			var currentTime = _now.UtcDateTime();

			PersonOrganizationData person;
			if (!_personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out person))
				return;
			person.BusinessUnitId = businessUnitId;

			var scheduleInfo = new ScheduleInfo(_databaseReader, person.PersonId, currentTime);

			var agentStateInfo = new AgentStateInfo(input, person, scheduleInfo, _agentStateAssembler, _databaseReader, currentTime);

			var info = new StateInfo(input, person, agentStateInfo, scheduleInfo, _alarmFinder);

			_actualAgentStateUpdater.Update(info);

			_messageSender.Send(info);

			_adherenceAggregator.Aggregate(info);

			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}
}