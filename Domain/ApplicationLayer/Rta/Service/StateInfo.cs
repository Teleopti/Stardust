using System;
using Teleopti.Ccc.Domain.ApplicationLayer.Events;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{

		public StateInfo(
			RtaProcessContext context,
			IStateMapper stateMapper,
			IDatabaseLoader databaseLoader,
			IAppliedAdherence appliedAdherence)
		{
			Input = context.Input;
			Person = context.Person;
			CurrentTime = context.CurrentTime;
			Stored = context.PreviousStateInfoLoader.Load(context.Person.PersonId);
			Schedule = new ScheduleInfo(databaseLoader, Person.PersonId, CurrentTime, Stored);
			State = new StateAlarmInfo(stateCode, platformTypeId, Input, Person, Stored, Schedule, stateMapper);
			Adherence = new AdherenceInfo(Input, Person, Stored, State, Schedule, appliedAdherence, stateMapper);
		}

		private Guid platformTypeId => string.IsNullOrEmpty(Input.PlatformTypeId) ?  Stored.PlatformTypeId() : Input.ParsedPlatformTypeId();

		private string stateCode => Input.StateCode ?? Stored.StateCode();

		private DateTime? stateStartTime => State.StateGroupChanged() ? CurrentTime : Stored.StateStartTime;

		private DateTime? adherenceStartTime => (Stored == null || State.Adherence() != Stored.Adherence) ? CurrentTime : Stored.AdherenceStartTime;

		private DateTime? batchId => Input.IsSnapshot ? Input.BatchId : Stored.BatchId();

		private DateTime? alarmStartTime
		{
			get
			{
				if (State.IsInAlarm())
				{
					if (State.HasRuleChanged())
						return CurrentTime.AddTicks(State.AlarmThresholdTime());
					return Stored.AlarmStartTime;
				}

				return null;
			}
		}

		public ExternalUserStateInputModel Input { get; set; }
		public PersonOrganizationData Person { get; }
		public StateAlarmInfo State { get; }
		public StoredStateInfo Stored { get; }
		public ScheduleInfo Schedule { get; }
		public AdherenceInfo Adherence { get; }
		public DateTime CurrentTime { get; }

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
				AdherenceStartTime = adherenceStartTime, 
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
		
		public EventAdherence AggregatorAdherence => Adherence.AdherenceForNewStateAndCurrentActivity();

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