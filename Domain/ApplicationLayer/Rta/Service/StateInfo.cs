using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		public StateInfo(
			RtaProcessContext context,
			IStateMapper stateMapper,
			IScheduleLoader scheduleLoader,
			IAppliedAdherence appliedAdherence)
		{
			input = context.Input;
			Person = context.Person;
			CurrentTime = context.CurrentTime;
			Stored = context.PreviousStateInfoLoader.Load(context.Person.PersonId);
			Schedule = new ScheduleInfo(scheduleLoader, Person.PersonId, CurrentTime, Stored);
			State = new StateAlarmInfo(stateCode, platformTypeId, input, Person, Stored, Schedule, stateMapper);
			Adherence = new AdherenceInfo(input, Person, Stored, State, Schedule, appliedAdherence, stateMapper);
		}

		private Guid platformTypeId { get { return string.IsNullOrEmpty(input.PlatformTypeId) ? Stored.PlatformTypeId : input.ParsedPlatformTypeId(); } }
		private string stateCode { get { return input.StateCode ?? Stored.StateCode; } }
		private DateTime? stateStart { get { return State.AlarmTypeId() == Stored.AlarmTypeId ? Stored.AlarmTypeStartTime : CurrentTime; } }
		private DateTime? batchId { get { return input.IsSnapshot ? input.BatchId : Stored.BatchId; } }

		private ExternalUserStateInputModel input { get; set; }
		public PersonOrganizationData Person { get; private set; }
		public StateAlarmInfo State { get; private set; }
		public StoredStateInfo Stored { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set;  }
		
		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return new AgentStateReadModel
			{
				BatchId = batchId,
				NextStart = Schedule.NextActivityStartTime(),
				OriginalDataSourceId = input.SourceId ?? Stored.SourceId,
				PersonId = Person.PersonId,
				PlatformTypeId = platformTypeId,
				ReceivedTime = CurrentTime,
				StaffingEffect = State.StaffingEffect(),
				Adherence = (int?)State.Adherence(),
				StateCode = stateCode,
				StateId = State.StateGroupId(),
				StateStart = stateStart,
				AlarmId = State.AlarmTypeId(),
				AlarmName = State.AlarmName(),
				AlarmStart = CurrentTime.AddTicks(State.AlarmThresholdTime()),
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







		public AdherenceState AggregatorAdherence { get { return Adherence.AdherenceState(); } }

		public bool Send
		{
			get
			{
				return !Schedule.CurrentActivityId().Equals(Stored.ActivityId) ||
					   !State.StateGroupId().Equals(Stored.StateGroupId) ||
					   !Schedule.NextActivityId().Equals(Stored.NextActivityId) ||
					   !Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime)
					;
			}
		}

	}
}