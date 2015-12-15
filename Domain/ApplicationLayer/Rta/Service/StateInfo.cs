using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly IAppliedAlarmStartTime _appliedAlarmStartTime;

		public StateInfo(
			RtaProcessContext context,
			IStateMapper stateMapper,
			IDatabaseLoader databaseLoader,
			IAppliedAdherence appliedAdherence,
			IAppliedAlarmStartTime appliedAlarmStartTime)
		{
			_appliedAlarmStartTime = appliedAlarmStartTime;
			Input = context.Input;
			Person = context.Person;
			CurrentTime = context.CurrentTime;
			Stored = context.PreviousStateInfoLoader.Load(context.Person.PersonId);
			Schedule = new ScheduleInfo(databaseLoader, Person.PersonId, CurrentTime, Stored);
			State = new StateAlarmInfo(stateCode, platformTypeId, Input, Person, Stored, Schedule, stateMapper);
			Adherence = new AdherenceInfo(Input, Person, Stored, State, Schedule, appliedAdherence, stateMapper);
		}

		private Guid platformTypeId
		{
		    get
		    {
		        return string.IsNullOrEmpty(Input.PlatformTypeId) ? Stored.PlatformTypeId() : Input.ParsedPlatformTypeId();
		    }
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
	        get
	        {
	            return (Stored == null || State.Adherence() != Stored.Adherence) ? CurrentTime : Stored.RuleStartTime;
	        }
	    }

	    private DateTime? batchId
	    {
	        get { return Input.IsSnapshot ? Input.BatchId : Stored.BatchId(); }
	    }

	    private DateTime? alarmStartTime
		{
			get
			{
				return _appliedAlarmStartTime.For(State, Stored, CurrentTime);
                
			}
		}

		public ExternalUserStateInputModel Input { get; set; }
		public PersonOrganizationData Person { get; private set; }
	    public StateAlarmInfo State { get; private set; }
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
				PersonId = Person.PersonId,
				PlatformTypeId = platformTypeId,
				ReceivedTime = CurrentTime,
				StaffingEffect = State.StaffingEffect(),
				Adherence = (int?)State.Adherence(),
				StateCode = stateCode,
				StateId = State.StateGroupId(),
				StateStartTime = stateStartTime,
				AlarmId = State.RuleId(),
				AlarmName = State.AlarmName(),
				RuleStartTime = ruleStartTime, 
				AlarmStartTime = alarmStartTime,
				BusinessUnitId = Person.BusinessUnitId,
				SiteId = Person.SiteId,
				TeamId = Person.TeamId,
				Color = State.AlarmDisplayColor(),
				Scheduled = Schedule.CurrentActivityName(),
				ScheduledId = Schedule.CurrentActivityId(),
				ScheduledNext = Schedule.NextActivityName(),
				ScheduledNextId = Schedule.NextActivityId(),
				State = State.StateGroupName()
			};
		}
		
		public EventAdherence AggregatorAdherence
		{
		    get { return Adherence.AdherenceForNewStateAndCurrentActivity(); }
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