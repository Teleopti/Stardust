using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly PersonOrganizationData _person;
		private readonly DateTime _currentTime;
		private readonly Lazy<AgentState> _previousState;
		private readonly Lazy<AgentState> _currentState;
		private readonly Lazy<Guid> _platformTypeId;
		private readonly Lazy<string> _stateCode;
		private readonly Lazy<StateMapping> _stateMapping;
		private readonly Lazy<AlarmMapping> _alarmMapping;
		private readonly ExternalUserStateInputModel _input;

		public StateInfo(
			RtaProcessContext context,
			IStateMapper stateMapper,
			IScheduleLoader scheduleLoader,
			IAppliedAdherence appliedAdherence)
		{
			var input = context.Input;
			var person = context.Person;
			var currentTime = context.CurrentTime;

			_input = input;
			_person = person;
			_currentTime = currentTime;
			_previousState = new Lazy<AgentState>(() => context.PreviousState(this));
			_currentState = new Lazy<AgentState>(() => context.CurrentState(this));

			Schedule = new ScheduleInfo(scheduleLoader, _person.PersonId, currentTime);
			Adherence = new AdherenceInfo(input, _person, () => _previousState.Value, () => _currentState.Value, Schedule, appliedAdherence, stateMapper);

			_platformTypeId = new Lazy<Guid>(() => string.IsNullOrEmpty(input.PlatformTypeId) ? _previousState.Value.PlatformTypeId : input.ParsedPlatformTypeId());
			_stateCode = new Lazy<string>(() => input.StateCode ?? _previousState.Value.StateCode);
			_stateMapping = new Lazy<StateMapping>(() => stateMapper.StateFor(person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, input.StateDescription));
			_alarmMapping = new Lazy<AlarmMapping>(() => stateMapper.AlarmFor(person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, Schedule.CurrentActivityId()) ?? new AlarmMapping());

		}

		public DateTime CurrentTime { get { return _currentTime; } }
		public string StateCode { get { return _stateCode.Value; } }
		public Guid PlatformTypeId { get { return _platformTypeId.Value; } }
		public Guid? StateGroupId { get { return _stateMapping.Value.StateGroupId; } }
		public Guid? AlarmTypeId { get { return _alarmMapping.Value.AlarmTypeId; } }
		public DateTime? AlarmTypeStartTime
		{
			get { return _alarmMapping.Value.AlarmTypeId == _previousState.Value.AlarmTypeId ? _previousState.Value.AlarmTypeStartTime : _currentTime; }
		}
		public double? StaffingEffect { get { return _alarmMapping.Value.StaffingEffect; } }
		public AdherenceState? AdherenceState2 { get { return _alarmMapping.Value.Adherence; } }
		public string AlarmName { get { return _alarmMapping.Value.AlarmName; } }
		public long AlarmThresholdTime { get { return _alarmMapping.Value.ThresholdTime; } }
		public int? AlarmDisplayColor { get { return _alarmMapping.Value.DisplayColor; } }
		public string StateGroupName { get { return _stateMapping.Value.StateGroupName; } }
		public DateTime? BatchId { get { return _input.IsSnapshot ? _input.BatchId : _previousState.Value.BatchId; } }
		public string SourceId
		{
			get { return _input.SourceId ?? _previousState.Value.SourceId; }
		}
		public Guid PersonId { get { return _person.PersonId; } }
		public Guid BusinessUnitId { get { return _person.BusinessUnitId; } }
		public Guid TeamId { get { return _person.TeamId; } }
		public Guid SiteId { get { return _person.SiteId; } }




		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }

		public bool IsScheduled { get { return _currentState.Value.ActivityId != null && CurrentActivity != null; } }
		public bool WasScheduled { get { return _previousState.Value.ActivityId != null && PreviousActivity != null; } }

		public ScheduleLayer CurrentActivity { get { return Schedule.CurrentActivity(); } }
		public ScheduleLayer PreviousActivity { get { return Schedule.PreviousActivity(); } }
		public ScheduleLayer NextActivityInShift { get { return Schedule.NextActivityInShift(); } }
		public DateTime CurrentShiftStartTime { get { return Schedule.CurrentShiftStartTime; } }
		public DateTime CurrentShiftEndTime { get { return Schedule.CurrentShiftEndTime; } }

		public DateTime ShiftStartTimeForPreviousActivity { get { return Schedule.ShiftStartTimeForPreviousActivity; } }
		public DateTime ShiftEndTimeForPreviousActivity { get { return Schedule.ShiftEndTimeForPreviousActivity; } }

		public AdherenceState AdherenceState { get { return Adherence.AdherenceState(); } }

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

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			var state = _currentState.Value;
			return new AgentStateReadModel
			{
				BatchId = state.BatchId,
				NextStart = state.NextActivityStartTime,
				OriginalDataSourceId = state.SourceId,
				PersonId = state.PersonId,
				PlatformTypeId = state.PlatformTypeId,
				ReceivedTime = state.ReceivedTime,
				StaffingEffect = state.StaffingEffect,
				Adherence = (int?) state.Adherence,
				StateCode = state.StateCode,
				StateId = state.StateGroupId,
				StateStart = state.AlarmTypeStartTime,
				AlarmId = AlarmTypeId,
				AlarmName = AlarmName,
				AlarmStart = CurrentTime.AddTicks(AlarmThresholdTime),
				BusinessUnitId = BusinessUnitId,
				SiteId = SiteId,
				TeamId = TeamId,
				Color = AlarmDisplayColor,
				Scheduled = Schedule.CurrentActivityName(),
				ScheduledId = Schedule.CurrentActivityId(),
				ScheduledNext = Schedule.NextActivityName(),
				ScheduledNextId = Schedule.NextActivityId(),
				State = StateGroupName
			};
		}
		
	}

}