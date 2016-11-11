using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Context
	{
		private readonly Action<Context> _updateState;
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<ScheduleState> _schedule;
		private readonly AgentStateFound _found;

		public Context(
			DateTime utcNow,
			DeadLockVictim deadLockVictim,
			InputInfo input, 
			Guid personId, 
			Guid businessUnitId, 
			Guid teamId, 
			Guid siteId, 
			Func<AgentState> stored, 
			Func<ScheduleState> schedule,
			IEnumerable<Mapping> mappings,
			Action<Context> updateState, 
			StateMapper stateMapper,
			ProperAlarm appliedAlarm)
		{

			// on synchronze, the func is null
			// and that is fine
			// it means there's no previous state
			if (stored != null)
			{
				Stored = stored.Invoke();
				_found = Stored as AgentStateFound;
				// if the stored state has no time
				// its just a prepared state
				// and there's no real previous state
				// throws if Stored is null, which is by design
				if (Stored.ReceivedTime == null)
					Stored = null;
			}

			Input = input ?? new InputInfo();
			CurrentTime = utcNow;
			DeadLockVictim = deadLockVictim;
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;
			StateMapper = stateMapper;

			_updateState = updateState ?? (c => {});
			_appliedAlarm = appliedAlarm;
			_schedule = new Lazy<ScheduleState>(schedule); ;

			var schedules = new Lazy<IEnumerable<ScheduledActivity>>(() => _schedule.Value.Schedules);
			Schedule = new ScheduleInfo(this, schedules);
			State = new StateRuleInfo(this, mappings);
			Adherence = new AdherenceInfo(this, mappings);
		}

		public DateTime CurrentTime { get; }

		public Guid PersonId { get; }
		public Guid BusinessUnitId { get; }
		public Guid TeamId { get; }
		public Guid SiteId { get; }

		public StateMapper StateMapper { get; set; }

		public InputInfo Input { get; set; }
		public AgentState Stored { get; set; }

		public StateRuleInfo State { get; }
		public ScheduleInfo Schedule { get; }
		public AdherenceInfo Adherence { get; }

		public bool ShouldProcessState()
		{
			// no previous state
			if (Stored == null)
				return true;

			// new schedules loaded
			if (_schedule.Value.NewSchedules)
				return true;

			var isSameState =
				SnapshotId.Equals(Stored.BatchId) &&
				Schedule.CurrentActivityId().Equals(Stored.ActivityId) &&
				State.StateGroupId().Equals(Stored.StateGroupId) &&
				Schedule.TimeWindowCheckSum().Equals(Stored.TimeWindowCheckSum)
				;

			return !isSameState;
		}

		public void UpdateAgentState()
		{
			_updateState(this);
		}

		// for logging
		public override string ToString()
		{
			return $"Time: {CurrentTime}, UserCode: {UserCode}, StateCode: {StateCode}, SourceId: {SourceId}, DataSourceId: {DataSourceId}, PersonId: {PersonId}, BusinessUnitId: {BusinessUnitId}, TeamId: {TeamId}, SiteId: {SiteId}";
		}

		public string UserCode => _found?.UserCode;
		public int? DataSourceId => _found?.DataSourceId;
		public string SourceId => Input.SourceId ?? Stored?.SourceId;
		public DateTime? SnapshotId => Input.SnapshotId ?? Stored?.BatchId;
		public Guid PlatformTypeId => string.IsNullOrEmpty(Input.PlatformTypeId) ? Stored.PlatformTypeId() : Input.ParsedPlatformTypeId();
		public string StateCode => Input.StateCode ?? Stored?.StateCode;
		public DateTime? StateStartTime => State.StateChanged() ? CurrentTime : Stored?.StateStartTime;
		public DateTime? RuleStartTime => State.RuleChanged() ? CurrentTime : Stored?.RuleStartTime;
		public bool IsAlarm => _appliedAlarm.IsAlarm(State);
		public DateTime? AlarmStartTime => _appliedAlarm.StartTime(State, Stored, CurrentTime);

		public bool CacheSchedules => _schedule.Value.NewSchedules;
		public DeadLockVictim DeadLockVictim { get; set; }

		public AgentState MakeAgentState()
		{
			return new AgentState
			{
				PersonId = PersonId,
				BusinessUnitId = BusinessUnitId,
				SiteId = SiteId,
				TeamId = TeamId,
				BatchId = SnapshotId,

				PlatformTypeId = PlatformTypeId,
				SourceId = SourceId,
				ReceivedTime = CurrentTime,

				StateCode = StateCode,
				StateGroupId = State.StateGroupId(),
				StateStartTime = StateStartTime,

				ActivityId = Schedule.CurrentActivityId(),

				RuleId = State.RuleId(),
				RuleStartTime = RuleStartTime,
				Adherence = Adherence.Adherence,

				AlarmStartTime = AlarmStartTime,

				TimeWindowCheckSum = Schedule.TimeWindowCheckSum(),

				Schedule = _schedule.Value.Schedules,
				NextCheck = Schedule.NextCheck()
			};
		}

	}
}