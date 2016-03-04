using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo
	{
		private readonly IAppliedAlarm _appliedAlarm;

		public StateInfo(
			StateContext context,
			StateMapper stateMapper,
			IAppliedAdherence appliedAdherence,
			IAppliedAlarm appliedAlarm)
		{
			_appliedAlarm = appliedAlarm;
			Input = context.Input;
			Person = context;
			CurrentTime = context.CurrentTime;
			Stored = context.Stored();
			Schedule = new ScheduleInfo(context, CurrentTime, Stored);
			State = new StateRuleInfo(stateCode, platformTypeId, Input, Person, Stored, Schedule, stateMapper);
			Adherence = new AdherenceInfo(Input, Person, Stored, State, Schedule, appliedAdherence, stateMapper);
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

		public ExternalUserStateInputModel Input { get; set; }
		public StateContext Person { get; private set; }
	    public StateRuleInfo State { get; private set; }
		public StoredStateInfo Stored { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set; }

		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return new AgentStateReadModel
			{
				BatchId = batchId,
				NextStart = Schedule.NextActivityStartTime(),
				OriginalDataSourceId = Input.SourceId ?? Stored.SourceId(),
				PlatformTypeId = platformTypeId,
				ReceivedTime = CurrentTime,
				PersonId = Person.PersonId,
				BusinessUnitId = Person.BusinessUnitId,
				SiteId = Person.SiteId,
				TeamId = Person.TeamId,

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