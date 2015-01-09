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

	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly ExternalUserStateInputModel _input;
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<bool> _personIsKnown;
		private readonly Lazy<AgentState> _newState;
		private readonly Lazy<Adherence> _adherence;
		private readonly Lazy<Adherence> _adherenceForPreviousState;
		private readonly Lazy<Adherence> _adherenceForPreviousStateAndCurrentActivity;
		private readonly Lazy<Adherence> _adherenceForNewStateAndPreviousActivity;

		private readonly PersonOrganizationData _person;
		private readonly IAlarmFinder _alarmFinder;
		private readonly ScheduleInfo _scheduleInfo;

		public StateInfo(
			IDatabaseReader databaseReader,
			AgentStateAssembler agentStateAssembler,
			IAlarmFinder alarmFinder,
			PersonOrganizationData person,
			ExternalUserStateInputModel input,
			DateTime currentTime)
		{
			_input = input;
			_person = person;
			_alarmFinder = alarmFinder;

			var previousActualAgentState = new Lazy<IActualAgentState>(() => databaseReader.GetCurrentActualAgentState(person.PersonId));
			_previousState = new Lazy<AgentState>(() => agentStateAssembler.MakePreviousState(person.PersonId, previousActualAgentState.Value));
			_personIsKnown = new Lazy<bool>(() => previousActualAgentState.Value != null);

			_scheduleInfo = new ScheduleInfo(databaseReader, person.PersonId, currentTime, _previousState.Value.ReceivedTime);

			_newState = new Lazy<AgentState>(() => agentStateAssembler.MakeCurrentState(_scheduleInfo, input, person, _previousState.Value, currentTime));

			_adherence = new Lazy<Adherence>(() => AdherenceFor(_newState.Value));
			_adherenceForPreviousState = new Lazy<Adherence>(() => !_personIsKnown.Value ? Adherence.None : AdherenceFor(_previousState.Value));
			_adherenceForPreviousStateAndCurrentActivity = new Lazy<Adherence>(() => adherenceFor(_previousState.Value.StateCode, _newState.Value.ActivityId));
			_adherenceForNewStateAndPreviousActivity = new Lazy<Adherence>(() => adherenceFor(_input.StateCode, _scheduleInfo.PreviousActivity()));

		}

		public bool IsScheduled { get { return _newState.Value.ActivityId != Guid.Empty; } }
		public bool WasScheduled { get { return _previousState.Value.ActivityId != Guid.Empty; } }

		public ScheduleLayer CurrentActivity { get { return _scheduleInfo.CurrentActivity(); } }
		public ScheduleLayer NextActivityInShift { get { return _scheduleInfo.NextActivityInShift(); } }
		public DateTime CurrentShiftStartTime { get { return _scheduleInfo.CurrentShiftStartTime; } }
		public DateTime CurrentShiftEndTime { get { return _scheduleInfo.CurrentShiftEndTime; } }

		public DateTime ShiftStartTimeForPreviousState { get { return _scheduleInfo.ShiftStartTimeForPreviousState; } }
		public DateTime ShiftEndTimeForPreviousState { get { return _scheduleInfo.ShiftEndTimeForPreviousState; } }

		public Adherence Adherence { get { return _adherence.Value; } }
		public Adherence AdherenceForPreviousState { get { return _adherenceForPreviousState.Value; } }
		public Adherence AdherenceForPreviousStateAndCurrentActivity { get { return _adherenceForPreviousStateAndCurrentActivity.Value; } }
		public Adherence AdherenceForNewStateAndPreviousActivity { get { return _adherenceForNewStateAndPreviousActivity.Value; } }

		public Guid PersonId { get { return _person.PersonId; } }
		public Guid BusinessUnitId { get { return _person.BusinessUnitId; } }
		public Guid TeamId { get { return _person.TeamId; } }
		public Guid SiteId { get { return _person.SiteId; } }

		public DateTime CurrentTime { get { return _newState.Value.ReceivedTime; } }
		public DateTime PreviousStateTime { get { return _previousState.Value.ReceivedTime; } }
		public Guid CurrentStateId { get { return _newState.Value.StateGroupId; } }
		public Guid PreviousStateId { get { return _previousState.Value.StateGroupId; } }
		public Guid CurrentActivityId { get { return _newState.Value.ActivityId; } }
		public Guid PreviousActivityId { get { return _previousState.Value.ActivityId; } }

		public bool Send
		{
			get
			{
				return !_newState.Value.ActivityId.Equals(_previousState.Value.ActivityId) ||
					   !_newState.Value.StateGroupId.Equals(_previousState.Value.StateGroupId) ||
					   !_newState.Value.NextActivityId.Equals(_previousState.Value.NextActivityId) ||
					   !_newState.Value.NextActivityStartTime.Equals(_previousState.Value.NextActivityStartTime)
					;
			}
		}

		public IActualAgentState MakeActualAgentState()
		{
			return _newState.Value.MakeActualAgentState();
		}

		private Adherence adherenceFor(string stateCode, ScheduleLayer activity)
		{
			if (activity == null)
				return Adherence.None;
			return adherenceFor(stateCode, activity.PayloadId);
		}

		private Adherence adherenceFor(string stateCode, Guid activityId)
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

		public static Adherence AdherenceFor(IActualAgentState state)
		{
			return adherenceFor(state.StaffingEffect);
		}

		private static Adherence adherenceFor(RtaAlarmLight alarm)
		{
			return adherenceFor(alarm.StaffingEffect);
		}

		private static Adherence adherenceFor(double staffingEffect)
		{
			return staffingEffect.Equals(0) ? Adherence.In : Adherence.Out;
		}

	}

}