using System;
using System.Collections.Generic;

namespace Teleopti.Wfm.Adherence.States
{
	public class Context
	{
		private readonly ProperAlarm _appliedAlarm;
		private readonly IEnumerable<ScheduledActivity> _schedule;
		private readonly InputInfo _input;

		public Context(
			DateTime time,
			DeadLockVictim deadLockVictim,
			InputInfo input,
			AgentState stored,
			IEnumerable<ScheduledActivity> schedule,
			StateMapper stateMapper,
			ExternalLogonMapper externalLogonMapper,
			BelongsToDateMapper belongsToDateMapper,
			ProperAlarm appliedAlarm)
		{
			Stored = stored;
			_input = input;
			Time = time;
			DeadLockVictim = deadLockVictim;
			PersonId = stored.PersonId;
			BusinessUnitId = stored.BusinessUnitId;
			TeamId = stored.TeamId.GetValueOrDefault();
			SiteId = stored.SiteId.GetValueOrDefault();
			StateMapper = stateMapper;
			ExternalLogonMapper = externalLogonMapper;
			BelongsToDateMapper = belongsToDateMapper;

			_appliedAlarm = appliedAlarm;
			_schedule = schedule;

			var schedules = new Lazy<IEnumerable<ScheduledActivity>>(() => _schedule);
			Schedule = new ScheduleInfo(this, schedules);
			State = new StateRuleInfo(this);
			Adherence = new AdherenceInfo(this);
		}

		public DateTime Time { get; }

		public Guid PersonId { get; }
		public Guid BusinessUnitId { get; }
		public Guid TeamId { get; }
		public Guid SiteId { get; }

		public StateMapper StateMapper { get; }
		public ExternalLogonMapper ExternalLogonMapper { get; }
		public BelongsToDateMapper BelongsToDateMapper { get; }

		public AgentState Stored { get; }

		public StateRuleInfo State { get; }
		public ScheduleInfo Schedule { get; }
		public AdherenceInfo Adherence { get; }

		public bool FirstTimeProcessingAgent() => Stored.ReceivedTime == null;

		public bool HasInput() => _input != null;

		public bool ShouldProcessState()
		{
			if (FirstTimeProcessingAgent())
				return true;
			if (State.StateChanged())
				return true;
			if (Schedule.ActivityChanged())
				return true;
			if (Schedule.TimeWindowCheckSum() != Stored.TimeWindowCheckSum)
				return true;
			if (SnapshotId != Stored.SnapshotId)
				return true;
			if (Schedule.ShiftStartsInOneHour())
				return true;
			return false;
		}

		// for logging
		public override string ToString() =>
			$"Time: {Time}, UserCode: {_input?.UserCode}, StateCode: {_input?.StateCode}, SourceId: {_input?.SourceId}, PersonId: {PersonId}, BusinessUnitId: {BusinessUnitId}, TeamId: {TeamId}, SiteId: {SiteId}";

		public string InputStateCode() => _input?.StateCode;
		public string InputStateDescription() => _input?.StateDescription;

		public DateTime? SnapshotId => _input?.SnapshotId ?? Stored.SnapshotId;
		public int? SnapshotDataSourceId => _input?.SnapshotDataSourceId ?? Stored.SnapshotDataSourceId;
		public DateTime? StateStartTime => State.StateChanged() ? Time : Stored.StateStartTime;
		public DateTime? RuleStartTime => State.RuleChanged() ? Time : Stored.RuleStartTime;
		public bool IsAlarm => _appliedAlarm.IsAlarm(State);
		public DateTime? AlarmStartTime => _appliedAlarm.StartTime(State, Stored, Time);

		public DeadLockVictim DeadLockVictim { get; }

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

				ReceivedTime = Time,

				StateGroupId = State.StateGroupId(),
				StateStartTime = StateStartTime,

				ActivityId = Schedule.CurrentActivityId(),

				RuleId = State.RuleId(),
				RuleStartTime = RuleStartTime,
				Adherence = Adherence.Adherence(),

				AlarmStartTime = AlarmStartTime,

				TimeWindowCheckSum = Schedule.TimeWindowCheckSum()
			};
		}
	}
}