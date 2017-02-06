using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Context
	{
		private readonly Action<Context> _updateState;
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _schedule;

		public Context(
			DateTime utcNow,
			DeadLockVictim deadLockVictim,
			InputInfo input, 
			AgentState stored, 
			Func<IEnumerable<ScheduledActivity>> schedule,
			Action<Context> updateState, 
			StateMapper stateMapper,
			ProperAlarm appliedAlarm)
		{
			Stored = stored;
			// if the stored state has no time
			// its just a prepared state
			// and there's no real previous state
			// throws if Stored is null, which is by design
			if (Stored.ReceivedTime == null)
				Stored = null;

			Input = input ?? new InputInfo();
			CurrentTime = utcNow;
			DeadLockVictim = deadLockVictim;
			PersonId = stored.PersonId;
			BusinessUnitId = stored.BusinessUnitId;
			TeamId = stored.TeamId.GetValueOrDefault();
			SiteId = stored.SiteId.GetValueOrDefault();
			StateMapper = stateMapper;

			_updateState = updateState ?? (c => {});
			_appliedAlarm = appliedAlarm;
			_schedule = new Lazy<IEnumerable<ScheduledActivity>>(schedule); ;

			var schedules = new Lazy<IEnumerable<ScheduledActivity>>(() => _schedule.Value);
			Schedule = new ScheduleInfo(this, schedules);
			State = new StateRuleInfo(this);
			Adherence = new AdherenceInfo(this);
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
			
			var isSameState =
				SnapshotId.Equals(Stored.SnapshotId) &&
				!Schedule.ActivityChanged() &&
				!State.StateChanged() &&
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
			return $"Time: {CurrentTime}, UserCode: {UserCode}, StateCode: {Input.StateCode}, SourceId: {SourceId}, PersonId: {PersonId}, BusinessUnitId: {BusinessUnitId}, TeamId: {TeamId}, SiteId: {SiteId}";
		}

		public string SourceId => Input.SourceId;
		public string UserCode => Input.UserCode;
		public DateTime? SnapshotId => Input.SnapshotId ?? Stored?.SnapshotId;
		public int? SnapshotDataSourceId => Input.SnapshotDataSourceId ??  Stored?.SnapshotDataSourceId;
		public Guid PlatformTypeId => string.IsNullOrEmpty(Input.PlatformTypeId) ? Stored.PlatformTypeId() : Input.ParsedPlatformTypeId();
		public DateTime? StateStartTime => State.StateChanged() ? CurrentTime : Stored?.StateStartTime;
		public DateTime? RuleStartTime => State.RuleChanged() ? CurrentTime : Stored?.RuleStartTime;
		public bool IsAlarm => _appliedAlarm.IsAlarm(State);
		public DateTime? AlarmStartTime => _appliedAlarm.StartTime(State, Stored, CurrentTime);

		public DeadLockVictim DeadLockVictim { get; set; }

		public AgentState MakeAgentState()
		{
			return new AgentState
			{
				PersonId = PersonId,
				BusinessUnitId = BusinessUnitId,
				SiteId = SiteId,
				TeamId = TeamId,
				SnapshotId = SnapshotId,
				SnapshotDataSourceId = SnapshotDataSourceId,

				PlatformTypeId = PlatformTypeId,
				ReceivedTime = CurrentTime,

				StateGroupId = State.StateGroupId(),
				StateStartTime = StateStartTime,

				ActivityId = Schedule.CurrentActivityId(),

				RuleId = State.RuleId(),
				RuleStartTime = RuleStartTime,
				Adherence = Adherence.Adherence,

				AlarmStartTime = AlarmStartTime,

				TimeWindowCheckSum = Schedule.TimeWindowCheckSum()
			};
		}

	}
}