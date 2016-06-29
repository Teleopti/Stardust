using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Helper;
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
		private readonly Action<Context> _updateState;
		private readonly ProperAlarm _appliedAlarm;
		private readonly Lazy<AgentState> _stored;

		public Context(
			ExternalUserStateInputModel input, 
			Guid personId, 
			Guid businessUnitId, 
			Guid teamId, 
			Guid siteId, 
			Func<AgentState> stored, 
			Func<IEnumerable<ScheduledActivity>> schedule,
			Func<Context, MappingsState> mappings,
			Action<Context> updateState, 
			INow now,
			StateMapper stateMapper,
			AppliedAdherence appliedAdherence,
			ProperAlarm appliedAlarm)
		{
			var dontDeferForNow = stored == null ? null : stored.Invoke();
			_stored = new Lazy<AgentState>(() => dontDeferForNow);
			var scheduleLazy = new Lazy<IEnumerable<ScheduledActivity>>(schedule);
			var mappingsState = mappings.Invoke(this);
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;

			_updateState = updateState ?? (c => {});
			_appliedAlarm = appliedAlarm;

			Schedule = new ScheduleInfo(scheduleLazy, _stored, CurrentTime);
			State = new StateRuleInfo(mappingsState, _stored, StateCode, PlatformTypeId, businessUnitId, Input, Schedule, stateMapper);
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

		public bool ShouldProcessState()
		{
			if (Stored == null)
				return true;
			
			var isSameState =
				BatchId.Equals(Stored?.BatchId) &&
				Schedule.CurrentActivityId().Equals(Stored?.ActivityId) &&
				Schedule.NextActivityId().Equals(Stored?.NextActivityId) &&
				Schedule.NextActivityStartTime().Equals(Stored?.NextActivityStartTime) &&
				State.StateGroupId().Equals(Stored?.StateGroupId) &&
				Schedule.TimeWindowCheckSum().Equals(Stored?.TimeWindowCheckSum)
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
			return string.Format(
				"PersonId: {0}, BusinessUnitId: {1}, TeamId: {2}, SiteId: {3}",
				PersonId, BusinessUnitId, TeamId, SiteId);
		}

		public DateTime? BatchId
		{
			get { return Input.IsSnapshot ? Input.BatchId : Stored?.BatchId; }
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
	        get { return State.StateGroupChanged() ? CurrentTime : Stored?.StateStartTime; }
	    }

		public DateTime? RuleStartTime
	    {
		    get { return State.HasRuleChanged() ? CurrentTime : Stored.RuleStartTime; }
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
				BatchId = BatchId,

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
				StaffingEffect = State.StaffingEffect(),
				Adherence = State.Adherence(),

				AlarmStartTime = AlarmStartTime,

				TimeWindowCheckSum = Schedule.TimeWindowCheckSum()
			};
		}
		
	}
}