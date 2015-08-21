using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.PulseLoop;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service.Aggregator;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class RtaProcessContext
	{
		private readonly Func<PreviousStateInfo> _previousState;
		private readonly Func<StateInfo, RtaProcessContext, CurrentAgentState> _currentState;
		private readonly PersonOrganizationData _person;
		private PreviousStateInfo _madePreviousStateInfo;

		public RtaProcessContext(
			ExternalUserStateInputModel input,
			Guid personId,
			Guid businessUnitId,
			DateTime currentTime,
			IPersonOrganizationProvider personOrganizationProvider,
			IAgentStateReadModelUpdater agentStateReadModelUpdater, 
			IAgentStateMessageSender messageSender, 
			IAdherenceAggregator adherenceAggregator,
			Func<PreviousStateInfo> previousState,
			Func<StateInfo, RtaProcessContext, CurrentAgentState> currentState 
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
			MessageSender = messageSender ?? new NoMessage();
			AdherenceAggregator = adherenceAggregator ?? new NoAggregation();
		}

		public ExternalUserStateInputModel Input { get; private set; }
		public PersonOrganizationData Person { get { return _person; } }
		public DateTime CurrentTime { get; private set; }

		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }
		public IAgentStateMessageSender MessageSender { get; private set; }
		public IAdherenceAggregator AdherenceAggregator { get; private set; }

		public PreviousStateInfo PreviousState(StateInfo info)
		{
			if (_madePreviousStateInfo != null)
				return _madePreviousStateInfo;
			_madePreviousStateInfo = _previousState.Invoke();
			return _madePreviousStateInfo;
		}

		public CurrentAgentState CurrentState(StateInfo info)
		{
			return _currentState.Invoke(info, this);
		}
	}
}