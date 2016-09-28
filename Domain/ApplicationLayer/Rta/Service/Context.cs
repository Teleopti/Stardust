using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class MappingsState
	{
		private readonly Func<IEnumerable<Mapping>> _loadMappings;
		private IEnumerable<Mapping> _mappings;

		public MappingsState(Func<IEnumerable<Mapping>> loadMappings)
		{
			_loadMappings = loadMappings;
		}

		public IEnumerable<Mapping> Use()
		{
			return _mappings ?? (_mappings = _loadMappings.Invoke());
		}

		public void Invalidate()
		{
			_mappings = null;
		}
	}

	public class Context
	{
		private readonly Action<Context> _updateState;
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<ScheduleState> _schedule;

		public Context(
			DateTime utcNow,
			InputInfo input, 
			Guid personId, 
			Guid businessUnitId, 
			Guid teamId, 
			Guid siteId, 
			Func<AgentState> stored, 
			Func<ScheduleState> schedule,
			Func<Context, MappingsState> mappings,
			Action<Context> updateState, 
			StateMapper stateMapper,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm)
		{

			// on synchronze, the func is null
			// and that is fine
			// it means there's no previous state
			if (stored != null)
			{
				Stored = stored.Invoke();
				// if the stored state has no time
				// its just a prepared state
				// and there's no real previous state
				// throws if Stored is null, which is by design
				if (Stored.ReceivedTime == null)
					Stored = null;
			}

			var mappingsState = mappings.Invoke(this);
			Input = input ?? new InputInfo();
			CurrentTime = utcNow;
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;
			AppliedAdherence = appliedAdherence;
			StateMapper = stateMapper;

			_updateState = updateState ?? (c => {});
			_appliedAlarm = appliedAlarm;
			_schedule = new Lazy<ScheduleState>(schedule); ;

			var schedules = new Lazy<IEnumerable<ScheduledActivity>>(() => _schedule.Value.Schedules);
			Schedule = new ScheduleInfo(this, schedules);
			State = new StateRuleInfo(this, mappingsState);
			Adherence = new AdherenceInfo(this, mappingsState);
		}

		public DateTime CurrentTime { get; private set; }

		public Guid PersonId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }

		public StateMapper StateMapper { get; set; }
		public AppliedAdherence AppliedAdherence { get; set; }

		public InputInfo Input { get; set; }
		public AgentState Stored { get; set; }

		public StateRuleInfo State { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }

		public bool ShouldProcessState()
		{
			if (Stored == null || _schedule.Value.CacheSchedules)
				return true;
			
			var isSameState =
				SnapshotId.Equals(Stored.BatchId) &&
				Schedule.CurrentActivityId().Equals(Stored.ActivityId) &&
				Schedule.NextActivityId().Equals(Stored.NextActivityId) &&
				Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime) &&
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
			return $"PersonId: {PersonId}, BusinessUnitId: {BusinessUnitId}, TeamId: {TeamId}, SiteId: {SiteId}";
		}

		public DateTime? SnapshotId
		{
			get { return Input.SnapshotId ?? Stored?.BatchId; }
		}

		public Guid PlatformTypeId
		{
			get { return string.IsNullOrEmpty(Input.PlatformTypeId) ? Stored.PlatformTypeId() : Input.ParsedPlatformTypeId(); }
		}

		public string StateCode
	    {
	        get { return Input.StateCode ?? Stored?.StateCode; }
	    }

		public DateTime? StateStartTime
	    {
	        get { return State.StateChanged() ? CurrentTime : Stored?.StateStartTime; }
	    }

		public DateTime? RuleStartTime
	    {
		    get { return State.RuleChanged() ? CurrentTime : Stored?.RuleStartTime; }
	    }

		public bool IsAlarm
		{
			get { return _appliedAlarm.IsAlarm(State); }
		}

		public DateTime? AlarmStartTime
		{
		    get { return _appliedAlarm.StartTime(State, Stored, CurrentTime); }
		}

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
				SourceId = Input.SourceId ?? Stored.SourceId(),
				ReceivedTime = CurrentTime,

				StateCode = StateCode,
				StateGroupId = State.StateGroupId(),
				StateStartTime = StateStartTime,

				ActivityId = Schedule.CurrentActivityId(),
				NextActivityId = Schedule.NextActivityId(),
				NextActivityStartTime = Schedule.NextActivityStartTime(),

				RuleId = State.RuleId(),
				RuleStartTime = RuleStartTime,
				Adherence = Adherence.Adherence,

				AlarmStartTime = AlarmStartTime,

				TimeWindowCheckSum = Schedule.TimeWindowCheckSum(),

				Schedule = _schedule.Value.CacheSchedules ? _schedule.Value.Schedules : null
			};
		}

	}
}