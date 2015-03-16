using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessContext
	{
		private readonly Func<AgentState> _previousState;
		private readonly Func<ScheduleInfo, RtaProcessContext, AgentState> _currentState;
		private readonly PersonOrganizationData _person;
		private AgentState _madePreviousState;

		public RtaProcessContext(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId,
			DateTime currentTime,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IAgentStateMessageSender messageSender, 
			IAdherenceAggregator adherenceAggregator,
			Func<AgentState> previousState,
			Func<ScheduleInfo, RtaProcessContext, AgentState> currentState 
			)
		{
			_previousState = previousState;
			_currentState = currentState;
			if (!personOrganizationProvider.PersonOrganizationData().TryGetValue(personId, out _person))
				return;
			_person.BusinessUnitId = businessUnitId;

			CurrentTime = currentTime;
			Input = input ?? new ExternalUserStateInputModel();
			AgentStateReadModelUpdater = agentStateReadModelUpdater ?? new DontUpdateAgentStateReadModel();
			MessageSender = messageSender ?? new NoMessagge();
			AdherenceAggregator = adherenceAggregator ?? new NoAggregation();
		}

		public ExternalUserStateInputModel Input { get; private set; }
		public PersonOrganizationData Person { get { return _person; } }
		public DateTime CurrentTime { get; private set; }

		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }
		public IAgentStateMessageSender MessageSender { get; private set; }
		public IAdherenceAggregator AdherenceAggregator { get; private set; }

		public AgentState PreviousState(ScheduleInfo scheduleInfo)
		{
			if (_madePreviousState != null)
				return _madePreviousState;
			_madePreviousState = _previousState.Invoke();
			return _madePreviousState;
		}

		public AgentState CurrentState(ScheduleInfo scheduleInfo)
		{
			return _currentState.Invoke(scheduleInfo, this);
		}
	}
}