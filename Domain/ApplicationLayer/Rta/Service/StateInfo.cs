using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo
	{
		private readonly IAppliedAlarm _appliedAlarm;
		private readonly Lazy<StoredStateInfo> _stored;
		private readonly Action<StateInfo> _agentStateReadModelUpdater;

		public StateInfo(
			ExternalUserStateInputModel input, 
			Guid personId, 
			Guid businessUnitId, 
			Guid teamId, 
			Guid siteId, 
			Func<StoredStateInfo> stored, 
			Func<IEnumerable<ScheduleLayer>> schedule, 
			Func<IEnumerable<Mapping>> mappings, 
			Action<StateInfo> agentStateReadModelUpdater, 
			INow now,
			StateMapper stateMapper,
			IAppliedAdherence appliedAdherence,
			IAppliedAlarm appliedAlarm)
		{
			_stored = new Lazy<StoredStateInfo>(() => stored == null ? null : stored.Invoke());
			var scheduleLazy = new Lazy<IEnumerable<ScheduleLayer>>(schedule);
			var mappingsLazy = new Lazy<IEnumerable<Mapping>>(mappings);
			_agentStateReadModelUpdater = agentStateReadModelUpdater ?? (a => { });
			Input = input ?? new ExternalUserStateInputModel();
			CurrentTime = now.UtcDateTime();
			PersonId = personId;
			BusinessUnitId = businessUnitId;
			TeamId = teamId;
			SiteId = siteId;

			_appliedAlarm = appliedAlarm;

			Schedule = new ScheduleInfo(scheduleLazy, _stored, CurrentTime);
			State = new StateRuleInfo(mappingsLazy, _stored, stateCode, platformTypeId, businessUnitId, Input, Schedule, stateMapper);
			Adherence = new AdherenceInfo(Input, _stored, mappingsLazy, businessUnitId, State, Schedule, appliedAdherence, stateMapper);
		}

		public Guid PersonId { get; private set; }
		public Guid BusinessUnitId { get; private set; }
		public Guid TeamId { get; private set; }
		public Guid SiteId { get; private set; }

		public ExternalUserStateInputModel Input { get; set; }
		public StoredStateInfo Stored { get { return _stored.Value; } }
		public StateRuleInfo State { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set; }

		public void UpdateAgentStateReadModel()
		{
			_agentStateReadModelUpdater.Invoke(this);
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
		
	    public bool Send
		{
			get
			{
				return !Schedule.CurrentActivityId().Equals(Stored.ActivityId()) ||
					   !State.StateGroupId().Equals(Stored.StateGroupId()) ||
					   !Schedule.NextActivityId().Equals(Stored.NextActivityId()) ||
					   !Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime())
					;
			}
		}
	}
}