using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly PersonOrganizationData _person;
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<AgentState> _currentState;
		private readonly ScheduleInfo _scheduleInfo;

		public StateInfo(PersonOrganizationData person, AgentStateInfo agentState, ScheduleInfo scheduleInfo, AdherenceInfo adherence)
		{
			_person = person;
			_scheduleInfo = scheduleInfo;
			Adherence = adherence;

			_previousState = new Lazy<AgentState>(agentState.PreviousState);
			_currentState = new Lazy<AgentState>(agentState.CurrentState);
		}

		public bool IsScheduled { get { return _currentState.Value.ActivityId != null && CurrentActivity != null; } }
		public bool WasScheduled { get { return _previousState.Value.ActivityId != null && PreviousActivity != null; } }

		public ScheduleLayer CurrentActivity { get { return _scheduleInfo.CurrentActivity(); } }
		public ScheduleLayer PreviousActivity { get { return _scheduleInfo.PreviousActivity(); } }
		public ScheduleLayer NextActivityInShift { get { return _scheduleInfo.NextActivityInShift(); } }
		public DateTime CurrentShiftStartTime { get { return _scheduleInfo.CurrentShiftStartTime; } }
		public DateTime CurrentShiftEndTime { get { return _scheduleInfo.CurrentShiftEndTime; } }

		public DateTime ShiftStartTimeForPreviousActivity { get { return _scheduleInfo.ShiftStartTimeForPreviousActivity; } }
		public DateTime ShiftEndTimeForPreviousActivity { get { return _scheduleInfo.ShiftEndTimeForPreviousActivity; } }

		public AdherenceState AdherenceState { get { return Adherence.CurrentAdherence(); } }
		public AdherenceInfo Adherence { get; private set; }

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

		public DateOnly? BelongsToDate
		{
			get { return _scheduleInfo.BelongsToDate; }
		}

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

	}

}