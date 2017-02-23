using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class Context
	{
		private readonly Action<Context> _updateState;
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<IEnumerable<ScheduledActivity>> _schedule;
		private readonly InputInfo _input;

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
			_input = input;
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

		public StateMapper StateMapper { get; }

		public AgentState Stored { get; }

		public StateRuleInfo State { get; }
		public ScheduleInfo Schedule { get; }
		public AdherenceInfo Adherence { get; }

		public bool FirstTimeProcessingAgent()
		{
			return Stored.ReceivedTime == null;
		}

		public bool HasInput()
		{
			return _input != null;
		}

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
			return false;
		}

		public void UpdateAgentState()
		{
			_updateState(this);
		}

		// for logging
		public override string ToString()
		{
			return $"Time: {CurrentTime}, UserCode: {_input?.UserCode}, StateCode: {_input?.StateCode}, SourceId: {_input?.SourceId}, PersonId: {PersonId}, BusinessUnitId: {BusinessUnitId}, TeamId: {TeamId}, SiteId: {SiteId}";
		}

		public string InputStateCode() => _input?.StateCode;
		public string InputStateDescription() => _input?.StateDescription;

		public DateTime? SnapshotId => _input?.SnapshotId ?? Stored.SnapshotId;
		public int? SnapshotDataSourceId => _input?.SnapshotDataSourceId ??  Stored.SnapshotDataSourceId;
		public DateTime? StateStartTime => State.StateChanged() ? CurrentTime : Stored.StateStartTime;
		public DateTime? RuleStartTime => State.RuleChanged() ? CurrentTime : Stored.RuleStartTime;
		public bool IsAlarm => _appliedAlarm.IsAlarm(State);
		public DateTime? AlarmStartTime => _appliedAlarm.StartTime(State, Stored, CurrentTime);

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