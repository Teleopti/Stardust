using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

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
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<AgentState> _stored;
		private readonly Action<Context> _agentStateReadModelUpdater;

		public Context(
			ExternalUserStateInputModel input, 
			Guid personId, 
			Guid businessUnitId, 
			Guid teamId, 
			Guid siteId, 
			Func<AgentState> stored, 
			Func<IEnumerable<ScheduleLayer>> schedule, 
			Func<Context, IEnumerable<Mapping>> mappings, 
			Action<Context> agentStateReadModelUpdater, 
			INow now,
			StateMapper stateMapper,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm)
		{
			var dontDeferForNow = stored == null ? null : stored.Invoke();
			_stored = new Lazy<AgentState>(() => dontDeferForNow);
			var scheduleLazy = new Lazy<IEnumerable<ScheduleLayer>>(schedule);
			var mappingsState = new MappingsState(() => mappings.Invoke(this));
			_agentStateReadModelUpdater = agentStateReadModelUpdater ?? (a => { });
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;

			_appliedAlarm = appliedAlarm;

			Schedule = new ScheduleInfo(scheduleLazy, _stored, CurrentTime);
			State = new StateRuleInfo(mappingsState, _stored, stateCode, platformTypeId, businessUnitId, Input, Schedule, stateMapper);
			Adherence = new AdherenceInfo(Input, _stored, mappingsState, businessUnitId, State, Schedule, appliedAdherence, stateMapper);
		}

		public Guid PersonId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }

		public ExternalUserStateInputModel Input { get; set; }
		public AgentState Stored { get { return _stored.Value; } }
		public StateRuleInfo State { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set; }

		public void UpdateAgentStateReadModel()
		{
			_agentStateReadModelUpdater.Invoke(this);
		}

		public bool ThereIsAChange()
		{
			return !batchId.Equals(Stored.BatchId()) ||
				   !Schedule.CurrentActivityId().Equals(Stored.ActivityId()) ||
				   !Schedule.NextActivityId().Equals(Stored.NextActivityId()) ||
				   !Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime()) ||
				   !State.StateGroupId().Equals(Stored.StateGroupId())
				;
		}

		// for logging
		public override string ToString()
		{
			return string.Format(
				"PersonId: {0}, BusinessUnitId: {1}, TeamId: {2}, SiteId: {3}",
				PersonId, BusinessUnitId, TeamId, SiteId);
		}

		private DateTime? batchId
		{
			get { return Input.IsSnapshot ? Input.BatchId : Stored.BatchId(); }
		}

		private Guid platformTypeId
		{
			get { return string.IsNullOrEmpty(Input.PlatformTypeId) ? Stored.PlatformTypeId() : Input.ParsedPlatformTypeId(); }
		}

	    private string stateCode
	    {
	        get { return Input.StateCode ?? Stored.StateCode(); }
	    }

	    private DateTime? stateStartTime
	    {
	        get { return State.StateGroupChanged() ? CurrentTime : Stored.StateStartTime; }
	    }

	    private DateTime? ruleStartTime
	    {
		    get { return State.HasRuleChanged() ? CurrentTime : Stored.RuleStartTime; }
	    }

		private bool isAlarm
		{
			get { return _appliedAlarm.IsAlarm(State); }
		}

		private DateTime? alarmStartTime
		{
		    get { return _appliedAlarm.StartTime(State, Stored, CurrentTime); }
		}

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return new AgentStateReadModel
			{
				BatchId = batchId,
				NextStart = Schedule.NextActivityStartTime(),
				OriginalDataSourceId = Input.SourceId ?? Stored.SourceId(),
				PlatformTypeId = platformTypeId,
				ReceivedTime = CurrentTime,
				PersonId = PersonId,
				BusinessUnitId = BusinessUnitId,
				SiteId = SiteId,
				TeamId = TeamId,

				Scheduled = Schedule.CurrentActivityName(),
				ScheduledId = Schedule.CurrentActivityId(),
				ScheduledNext = Schedule.NextActivityName(),
				ScheduledNextId = Schedule.NextActivityId(),

				StateCode = stateCode,
				StateId = State.StateGroupId(),
				StateName = State.StateGroupName(),
				StateStartTime = stateStartTime,

				RuleId = State.RuleId(),
				RuleName = State.RuleName(),
				RuleStartTime = ruleStartTime,
				RuleColor = State.RuleDisplayColor(),
				StaffingEffect = State.StaffingEffect(),
				Adherence = (int?)State.Adherence(),

				IsAlarm = isAlarm,
				AlarmStartTime = alarmStartTime,
				AlarmColor = State.AlarmColor(),
			};
		}
		
	}
}