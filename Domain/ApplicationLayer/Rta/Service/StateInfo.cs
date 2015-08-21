using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class StateInfo : IAdherenceAggregatorInfo
	{
		private readonly Lazy<Guid> _platformTypeId;
		private readonly Lazy<string> _stateCode;
		private readonly Lazy<StateMapping> _stateMapping;
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

			_stateMapping = new Lazy<StateMapping>(() => stateMapper.StateFor(Person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, input.StateDescription));
			_alarmMapping = new Lazy<AlarmMapping>(() => stateMapper.AlarmFor(Person.BusinessUnitId, _platformTypeId.Value, _stateCode.Value, Schedule.CurrentActivityId()) ?? new AlarmMapping());

			Schedule = new ScheduleInfo(scheduleLoader, Person.PersonId, CurrentTime, Stored);
			Adherence = new AdherenceInfo(input, Person, Stored, _alarmMapping, Schedule, appliedAdherence, stateMapper);

			_platformTypeId = new Lazy<Guid>(() => string.IsNullOrEmpty(input.PlatformTypeId) ? Stored.PlatformTypeId : input.ParsedPlatformTypeId());
			_stateCode = new Lazy<string>(() => input.StateCode ?? Stored.StateCode);

		}

		private ExternalUserStateInputModel input { get; set; }
		public PersonOrganizationData Person { get; private set; }
		public StoredStateInfo Stored { get; private set; }
		public ScheduleInfo Schedule { get; private set; }
		public AdherenceInfo Adherence { get; private set; }
		public DateTime CurrentTime { get; private set;  }
		
		public AdherenceState AdherenceState { get { return Adherence.AdherenceState(); } }
		public Guid? CurrentStateId { get { return _stateMapping.Value.StateGroupId; } }

		public bool Send
		{
			get
			{
				return !Schedule.CurrentActivityId().Equals(Stored.ActivityId) ||
					   !_stateMapping.Value.StateGroupId.Equals(Stored.StateGroupId) ||
					   !Schedule.NextActivityId().Equals(Stored.NextActivityId) ||
					   !Schedule.NextActivityStartTime().Equals(Stored.NextActivityStartTime)
					;
			}
		}

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
				StateId = _stateMapping.Value.StateGroupId,
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
				State = _stateMapping.Value.StateGroupName
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
	}

}