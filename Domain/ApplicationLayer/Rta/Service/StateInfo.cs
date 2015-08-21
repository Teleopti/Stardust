using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly Lazy<Guid> _platformTypeId;
		private readonly Lazy<string> _stateCode;
		private readonly StateMapping _stateMapping;
		private readonly Lazy<AlarmMapping> _alarmMapping;

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

			_stateCode = new Lazy<string>(() => input.StateCode ?? Stored.StateCode);
			_platformTypeId = new Lazy<Guid>(() => string.IsNullOrEmpty(input.PlatformTypeId) ? Stored.PlatformTypeId : input.ParsedPlatformTypeId());

			_stateMapping = stateMapper.StateFor(Person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, input.StateDescription);
			_alarmMapping = new Lazy<AlarmMapping>(() => stateMapper.AlarmFor(Person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, Schedule.CurrentActivityId()) ?? new AlarmMapping());

			State = new StateInfo2(_stateMapping, Stored);
			Schedule = new ScheduleInfo(scheduleLoader, Person.PersonId, CurrentTime, Stored);
			Adherence = new AdherenceInfo(input, Person, Stored, _alarmMapping, Schedule, appliedAdherence, stateMapper);
		}

		private ExternalUserStateInputModel input { get; set; }
		public PersonOrganizationData Person { get; private set; }
		public StateInfo2 State { get; private set; }
		public StoredStateInfo Stored { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set;  }
		
		public AgentStateReadModel MakeAgentStateReadModel()
		{
			return new AgentStateReadModel
			{
				BatchId = batchId(),
				NextStart = Schedule.NextActivityStartTime(),
				OriginalDataSourceId = input.SourceId ?? Stored.SourceId,
				PersonId = Person.PersonId,
				PlatformTypeId = _platformTypeId.Value,
				ReceivedTime = CurrentTime,
				StaffingEffect = _alarmMapping.Value.StaffingEffect,
				Adherence = (int?)_alarmMapping.Value.Adherence,
				StateCode = _stateCode.Value,
				StateId = _stateMapping.StateGroupId,
				StateStart = stateStart(),
				AlarmId = _alarmMapping.Value.AlarmTypeId,
				AlarmName = _alarmMapping.Value.AlarmName,
				AlarmStart = CurrentTime.AddTicks(_alarmMapping.Value.ThresholdTime),
				BusinessUnitId = Person.BusinessUnitId,
				SiteId = Person.SiteId,
				TeamId = Person.TeamId,
				Color = _alarmMapping.Value.DisplayColor,
				Scheduled = Schedule.CurrentActivityName(),
				ScheduledId = Schedule.CurrentActivityId(),
				ScheduledNext = Schedule.NextActivityName(),
				ScheduledNextId = Schedule.NextActivityId(),
				State = _stateMapping.StateGroupName
			};
		}

		private DateTime? batchId()
		{
			return input.IsSnapshot ? input.BatchId : Stored.BatchId;
		}

		private DateTime? stateStart()
		{
			return _alarmMapping.Value.AlarmTypeId == Stored.AlarmTypeId ? Stored.AlarmTypeStartTime : CurrentTime;
		}






		public AdherenceState AggregatorAdherence { get { return Adherence.AdherenceState(); } }

		public bool Send
		{
			get
			{
				return !Schedule.CurrentActivityId().Equals(Stored.ActivityId) ||
					   !_stateMapping.StateGroupId.Equals(Stored.StateGroupId) ||
					   !Schedule.NextActivityId().Equals(Stored.NextActivityId) ||
					   !Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime)
					;
			}
		}

	}
}