using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly PersonOrganizationData _person;
		private readonly DateTime _currentTime;
		private readonly Lazy<CurrentAgentState> _currentState;
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

			Previous = context.PreviousState(this);

			_currentState = new Lazy<CurrentAgentState>(() => context.CurrentState(this));

			Schedule = new ScheduleInfo(scheduleLoader, _person.PersonId, currentTime, Previous);
			Adherence = new AdherenceInfo(input, _person, Previous, () => _currentState.Value, Schedule, appliedAdherence, stateMapper);

			_platformTypeId = new Lazy<Guid>(() => string.IsNullOrEmpty(input.PlatformTypeId) ? Previous.PlatformTypeId : input.ParsedPlatformTypeId());
			_stateCode = new Lazy<string>(() => input.StateCode ?? Previous.StateCode);
			_stateMapping = new Lazy<StateMapping>(() => stateMapper.StateFor(person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, input.StateDescription));
			_alarmMapping = new Lazy<AlarmMapping>(() => stateMapper.AlarmFor(person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, Schedule.CurrentActivityId()) ?? new AlarmMapping());

		}

		public DateTime CurrentTime { get { return _currentTime; } }
		public string StateCode { get { return _stateCode.Value; } }
		public Guid PlatformTypeId { get { return _platformTypeId.Value; } }
		public Guid? StateGroupId { get { return _stateMapping.Value.StateGroupId; } }
		public Guid? AlarmTypeId { get { return _alarmMapping.Value.AlarmTypeId; } }
		public DateTime? AlarmTypeStartTime { get { return _alarmMapping.Value.AlarmTypeId == Previous.AlarmTypeId ? Previous.AlarmTypeStartTime : _currentTime; } }
		public double? StaffingEffect { get { return _alarmMapping.Value.StaffingEffect; } }
		public AdherenceState? AdherenceState2 { get { return _alarmMapping.Value.Adherence; } }
		public string AlarmName { get { return _alarmMapping.Value.AlarmName; } }
		public long AlarmThresholdTime { get { return _alarmMapping.Value.ThresholdTime; } }
		public int? AlarmDisplayColor { get { return _alarmMapping.Value.DisplayColor; } }
		public string StateGroupName { get { return _stateMapping.Value.StateGroupName; } }
		public DateTime? BatchId { get { return _input.IsSnapshot ? _input.BatchId : Previous.BatchId; } }
		public string SourceId { get { return _input.SourceId ?? Previous.SourceId; } }
		public Guid PersonId { get { return _person.PersonId; } }
		public Guid BusinessUnitId { get { return _person.BusinessUnitId; } }
		public Guid TeamId { get { return _person.TeamId; } }
		public Guid SiteId { get { return _person.SiteId; } }




		public PreviousStateInfo Previous { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }

		public ScheduleLayer CurrentActivity { get { return Schedule.CurrentActivity(); } }
		public ScheduleLayer PreviousActivity { get { return Schedule.PreviousActivity(); } }
		public ScheduleLayer NextActivityInShift { get { return Schedule.NextActivityInShift(); } }
		public DateTime CurrentShiftStartTime { get { return Schedule.CurrentShiftStartTime; } }
		public DateTime CurrentShiftEndTime { get { return Schedule.CurrentShiftEndTime; } }

		public DateTime ShiftStartTimeForPreviousActivity { get { return Schedule.ShiftStartTimeForPreviousActivity; } }
		public DateTime ShiftEndTimeForPreviousActivity { get { return Schedule.ShiftEndTimeForPreviousActivity; } }

		public AdherenceState AdherenceState { get { return Adherence.AdherenceState(); } }

		public Guid? CurrentStateId { get { return StateGroupId; } }
		public Guid? PreviousStateId { get { return Previous.StateGroupId; } }
		public Guid? PreviousActivityId { get { return Previous.ActivityId; } }

		public bool Send
		{
			get
			{
				return !Schedule.CurrentActivityId().Equals(Previous.ActivityId) ||
					   !StateGroupId.Equals(Previous.StateGroupId) ||
					   !_currentState.Value.NextActivityId.Equals(Previous.NextActivityId) ||
					   !_currentState.Value.NextActivityStartTime.Equals(Previous.NextActivityStartTime)
					;
			}
		}

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			var state = _currentState.Value;
			return new AgentStateReadModel
			{
				BatchId = BatchId,
				NextStart = state.NextActivityStartTime,
				OriginalDataSourceId = SourceId,
				PersonId = PersonId,
				PlatformTypeId = PlatformTypeId,
				ReceivedTime = CurrentTime,
				StaffingEffect = state.StaffingEffect,
				Adherence = (int?) state.Adherence,
				StateCode = StateCode,
				StateId = StateGroupId,
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