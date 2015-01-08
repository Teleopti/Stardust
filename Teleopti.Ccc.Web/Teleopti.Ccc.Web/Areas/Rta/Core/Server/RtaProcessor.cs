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
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IActualAgentAssembler _agentAssembler;
		private readonly IAlarmFinder _alarmFinder;
		private readonly IAgentStateMessageSender _messageSender;
		private readonly IAdherenceAggregator _adherenceAggregator;
		private readonly IShiftEventPublisher _shiftEventPublisher;
		private readonly IActivityEventPublisher _activityEventPublisher;
		private readonly IStateEventPublisher _stateEventPublisher;

		public RtaProcessor(
			INow now,
			IPersonOrganizationProvider personOrganizationProvider, 
			IDatabaseReader databaseReader,
			IDatabaseWriter databaseWriter,
			IActualAgentAssembler agentAssembler,
			IAlarmFinder alarmFinder,
			IAgentStateMessageSender messageSender,
			IAdherenceAggregator adherenceAggregator,
			IShiftEventPublisher shiftEventPublisher,
			IActivityEventPublisher activityEventPublisher,
			IStateEventPublisher stateEventPublisher
			)
		{
			_now = now;
			_personOrganizationProvider = personOrganizationProvider;
			_databaseReader = databaseReader;
			_databaseWriter = databaseWriter;
			_agentAssembler = agentAssembler;
			_alarmFinder = alarmFinder;
			_messageSender = messageSender;
			_adherenceAggregator = adherenceAggregator;
			_shiftEventPublisher = shiftEventPublisher;
			_activityEventPublisher = activityEventPublisher;
			_stateEventPublisher = stateEventPublisher;
		}

		public void Process(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId
			)
		{

			PersonOrganizationData person;
			if (!_personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out person))
				return;
			person.BusinessUnitId = businessUnitId;

			var info = new StateInfo(
				_databaseReader,
				_agentAssembler,
				_alarmFinder,
				person,
				input,
				_now.UtcDateTime()
				);

			_databaseWriter.PersistActualAgentState(info.NewState);

			if (info.Send)
				_messageSender.Send(info);

			_adherenceAggregator.Aggregate(info);

			_shiftEventPublisher.Publish(info);
			_activityEventPublisher.Publish(info);
			_stateEventPublisher.Publish(info);
		}
	}
}