using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Ccc.Infrastructure.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.Rta.Core.Server.Adherence;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class RtaProcessContext
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly AgentStateAssembler _agentStateAssembler;
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
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			Func<AgentState> previousState,
			Func<ScheduleInfo, RtaProcessContext, AgentState> currentState 
			)
		{
			_databaseReader = databaseReader;
			_agentStateAssembler = agentStateAssembler;
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
			
			//_previousState = new Lazy<AgentState>(() => _agentStateAssembler.MakePreviousState(Person.PersonId, _databaseReader.GetCurrentActualAgentState(Person.PersonId)));
			//_currentState = new Lazy<AgentState>(() => currentState.Invoke(previousState));
		}

		public ExternalUserStateInputModel Input { get; private set; }
		public PersonOrganizationData Person { get { return _person; } }
		public DateTime CurrentTime { get; private set; }

		public IAgentStateReadModelUpdater AgentStateReadModelUpdater { get; private set; }
		public IAgentStateMessageSender MessageSender { get; private set; }
		public IAdherenceAggregator AdherenceAggregator { get; private set; }

		public AgentState MakePreviousState(ScheduleInfo scheduleInfo)
		{
			if (_madePreviousState != null)
				return _madePreviousState;
			_madePreviousState = _previousState.Invoke();
			return _madePreviousState;
		}

		public AgentState MakeCurrentState(ScheduleInfo scheduleInfo)
		{
			return _currentState.Invoke(scheduleInfo, this);
			//if (_makeCurrentState == null)
			//	_makeCurrentState = () => _agentStateAssembler.MakeCurrentState(scheduleInfo, Input, Person, _previousState.Value, CurrentTime);
			//return _makeCurrentState.Invoke();
		}

		//public void SetStuffUp(AgentStateReadModel previousState)
		//{
		//	_previousState = new Lazy<AgentState>(() => _agentStateAssembler.MakeEmpty(Person.PersonId));
		//	_makeCurrentState = () => _agentStateAssembler.MakeCurrentStateFromPrevious(previousState);
		//}
	}
}