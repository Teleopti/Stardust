using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public enum Adherence
	{
		None,
		In,
		Out
	}

	public class AgentStateInfo
	{
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<AgentState> _currentState;

		public AgentStateInfo(
			ExternalUserStateInputModel input,
			PersonOrganizationData person,
			ScheduleInfo scheduleInfo, 
			AgentStateAssembler agentStateAssembler,
			IDatabaseReader databaseReader,
			DateTime currentTime
			)
		{
			var previousActualAgentState = new Lazy<AgentStateReadModel>(() => databaseReader.GetCurrentActualAgentState(person.PersonId));
			_previousState = new Lazy<AgentState>(() => agentStateAssembler.MakePreviousState(person.PersonId, previousActualAgentState.Value));
			_currentState = new Lazy<AgentState>(() => agentStateAssembler.MakeCurrentState(scheduleInfo, input, person, _previousState.Value, currentTime));
		}

		public AgentState CurrentState()
		{
			return _currentState.Value;
		}

		public AgentState PreviousState()
		{
			return _previousState.Value;
		}
	}

	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly PersonOrganizationData _person;
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<AgentState> _currentState;
		private readonly ScheduleInfo _scheduleInfo;

		private readonly IAlarmFinder _alarmFinder;

		private readonly Lazy<Adherence> _adherence;
		private readonly Lazy<Adherence> _adherenceForPreviousState;
		private readonly Lazy<Adherence> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<Adherence> _adherenceForNewStateAndPreviousActivity;

		public StateInfo(ExternalUserStateInputModel input, PersonOrganizationData person, AgentStateInfo agentState, ScheduleInfo scheduleInfo, IAlarmFinder alarmFinder)
		{
			_input = input;
			_person = person;
			_scheduleInfo = scheduleInfo;
			_alarmFinder = alarmFinder;

			_previousState = new Lazy<AgentState>(agentState.PreviousState);
			_currentState = new Lazy<AgentState>(agentState.CurrentState);

			_adherence = new Lazy<Adherence>(() => AdherenceFor(_currentState.Value));
			_adherenceForPreviousState = new Lazy<Adherence>(() => AdherenceFor(_previousState.Value));
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<Adherence>(() => adherenceFor(_previousState.Value.StateCode, _currentState.Value.ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<Adherence>(() => adherenceFor(_input.StateCode, _scheduleInfo.PreviousActivity()));
		}

		public bool IsScheduled { get { return _currentState.Value.ActivityId != null; } }
		public bool WasScheduled { get { return _previousState.Value.ActivityId != null; } }

		public ScheduleLayer CurrentActivity { get { return _scheduleInfo.CurrentActivity(); } }
		public ScheduleLayer NextActivityInShift { get { return _scheduleInfo.NextActivityInShift(); } }
		public DateTime CurrentShiftStartTime { get { return _scheduleInfo.CurrentShiftStartTime; } }
		public DateTime CurrentShiftEndTime { get { return _scheduleInfo.CurrentShiftEndTime; } }

		public DateTime ShiftStartTimeForPreviousActivity { get { return _scheduleInfo.ShiftStartTimeForPreviousActivity; } }
		public DateTime ShiftEndTimeForPreviousActivity { get { return _scheduleInfo.ShiftEndTimeForPreviousActivity; } }

		public Adherence Adherence { get { return _adherence.Value; } }
		public Adherence AdherenceForPreviousState { get { return _adherenceForPreviousState.Value; } }
		public Adherence AdherenceForPreviousStateAndCurrentActivity { get { return _adherenceForPreviousStateAndCurrentActivity.Value; } }
		public Adherence AdherenceForNewStateAndPreviousActivity { get { return _adherenceForNewStateAndPreviousActivity.Value; } }

		public Guid PersonId { get { return _person.PersonId; } }
		public Guid BusinessUnitId { get { return _person.BusinessUnitId; } }
		public Guid TeamId { get { return _person.TeamId; } }
		public Guid SiteId { get { return _person.SiteId; } }

		public DateTime CurrentTime { get { return _currentState.Value.ReceivedTime; } }
		public DateTime PreviousStateTime { get { return _previousState.Value.ReceivedTime; } }
		public Guid? CurrentStateId { get { return _currentState.Value.StateGroupId; } }
		public Guid? PreviousStateId { get { return _previousState.Value.StateGroupId; } }
		public Guid? CurrentActivityId { get { return _currentState.Value.ActivityId; } }
		public Guid? PreviousActivityId { get { return _previousState.Value.ActivityId; } }

		public bool Send
		{
			get
			{
				return !_currentState.Value.ActivityId.Equals(_previousState.Value.ActivityId) ||
					   !_currentState.Value.StateGroupId.Equals(_previousState.Value.StateGroupId) ||
					   !_currentState.Value.NextActivityId.Equals(_previousState.Value.NextActivityId) ||
					   !_currentState.Value.NextActivityStartTime.Equals(_previousState.Value.NextActivityStartTime)
					;
			}
		}

		public AgentStateReadModel MakeActualAgentState()
		{
			return _currentState.Value.MakeActualAgentState();
		}

		private Adherence adherenceFor(string stateCode, ScheduleLayer activity)
		{
			if (activity == null)
				return Adherence.None;
			return adherenceFor(stateCode, activity.PayloadId);
		}

		private Adherence adherenceFor(string stateCode, Guid? activityId)
		{
			var stateGroup = _alarmFinder.GetStateGroup(
				stateCode,
				_input.ParsedPlatformTypeId(),
				_person.BusinessUnitId);
			var alarm = _alarmFinder.GetAlarm(activityId, stateGroup.StateGroupId, _person.BusinessUnitId);
			if (alarm == null)
				return Adherence.None;
			return adherenceFor(alarm);
		}

		public static Adherence AdherenceFor(AgentState state)
		{
			return adherenceFor(state.StaffingEffect);
		}

		public static Adherence AdherenceFor(AgentStateReadModel stateReadModel)
		{
			return adherenceFor(stateReadModel.StaffingEffect);
		}

		private static Adherence adherenceFor(RtaAlarmLight alarm)
		{
			return adherenceFor(alarm.StaffingEffect);
		}

		private static Adherence adherenceFor(double? staffingEffect)
		{
			if (staffingEffect.HasValue)
				return staffingEffect.Value.Equals(0) ? Adherence.In : Adherence.Out;
			return Adherence.None;
		}

	}

}