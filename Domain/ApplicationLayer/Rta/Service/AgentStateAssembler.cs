using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateAssembler
	{
		private readonly IStateMapper _stateMapper;

		public AgentStateAssembler(IStateMapper stateMapper)
		{
			_stateMapper = stateMapper;
		}

		public AgentState MakeEmpty(Guid personId)
		{
			return new AgentState
			{
				PersonId = personId,
				StateGroupId = Guid.NewGuid()
			};
		}

		public AgentState MakeCurrentStateFromPrevious(AgentStateReadModel fromStorage)
		{
			return new AgentState
			{
				PersonId = fromStorage.PersonId,
				BatchId = fromStorage.BatchId,
				PlatformTypeId = fromStorage.PlatformTypeId,
				SourceId = fromStorage.OriginalDataSourceId,
				ReceivedTime = fromStorage.ReceivedTime,
				StateCode = fromStorage.StateCode,
				StateGroupId = fromStorage.StateId,
				ActivityId = fromStorage.ScheduledId,
				NextActivityId = fromStorage.ScheduledNextId,
				NextActivityStartTime = fromStorage.NextStart,
				AlarmTypeId = fromStorage.AlarmId,
				AlarmTypeStartTime = fromStorage.StateStart,
				StaffingEffect = fromStorage.StaffingEffect,
				Adherence = (AdherenceState?) fromStorage.Adherence
			};
		}

		public AgentState MakePreviousState(Guid personId, AgentStateReadModel fromStorage)
		{
			if (fromStorage == null)
				return MakeEmpty(personId);
			return new AgentState
			{
				PersonId = fromStorage.PersonId,
				BatchId = fromStorage.BatchId,
				PlatformTypeId = fromStorage.PlatformTypeId,
				SourceId = fromStorage.OriginalDataSourceId,
				ReceivedTime = fromStorage.ReceivedTime,
				StateCode = fromStorage.StateCode,
				StateGroupId = fromStorage.StateId,
				ActivityId = fromStorage.ScheduledId,
				NextActivityId = fromStorage.ScheduledNextId,
				NextActivityStartTime = fromStorage.NextStart,
				AlarmTypeId = fromStorage.AlarmId,
				AlarmTypeStartTime = fromStorage.StateStart,
				StaffingEffect = fromStorage.StaffingEffect,
				Adherence = (AdherenceState?) fromStorage.Adherence
			};
		}

		public AgentState MakeCurrentState(ScheduleInfo scheduleInfo, ExternalUserStateInputModel input, PersonOrganizationData person, AgentState previous, DateTime currentTime)
		{
			var platformTypeId = string.IsNullOrEmpty(input.PlatformTypeId) ?  previous.PlatformTypeId : input.ParsedPlatformTypeId();
			var stateCode = input.StateCode ?? previous.StateCode;
			var state = _stateMapper.StateFor(person.BusinessUnitId, platformTypeId, stateCode, input.StateDescription);
			var alarm = _stateMapper.AlarmFor(person.BusinessUnitId, platformTypeId, stateCode, scheduleInfo.CurrentActivityId()) ?? new AlarmMapping();
			var agentState = new AgentState
			{
				PersonId = person.PersonId,
				BatchId = input.IsSnapshot ? input.BatchId : previous.BatchId,
				PlatformTypeId = platformTypeId,
				SourceId = input.SourceId ?? previous.SourceId,
				ReceivedTime = currentTime,
				StateCode = stateCode,
				StateGroupId = state.StateGroupId,
				ActivityId = scheduleInfo.CurrentActivityId(),
				AlarmTypeId = alarm.AlarmTypeId,
				AlarmTypeStartTime = alarm.AlarmTypeId == previous.AlarmTypeId ? previous.AlarmTypeStartTime : currentTime,
				NextActivityId = scheduleInfo.NextActivityId(),
				NextActivityStartTime = scheduleInfo.NextActivityStartTime(),
				StaffingEffect = alarm.StaffingEffect,
				Adherence = alarm.Adherence
			};
			agentState.UseAssembleMethod(() =>
			{
				var model = ReadModelFromState(agentState);
				model.AlarmId = alarm.AlarmTypeId;
				model.AlarmName = alarm.AlarmName;
				model.AlarmStart = currentTime.AddTicks(alarm.ThresholdTime);
				model.BusinessUnitId = person.BusinessUnitId;
				model.SiteId = person.SiteId;
				model.TeamId = person.TeamId;
				model.Color = alarm.DisplayColor;
				model.Scheduled = scheduleInfo.CurrentActivityName();
				model.ScheduledId = scheduleInfo.CurrentActivityId();
				model.ScheduledNext = scheduleInfo.NextActivityName();
				model.ScheduledNextId = scheduleInfo.NextActivityId();
				model.State = state.StateGroupName;
				return model;
			});
			return agentState;
		}

		public static AgentStateReadModel ReadModelFromState(AgentState agentState)
		{
			return new AgentStateReadModel
			{
				BatchId = agentState.BatchId,
				NextStart = agentState.NextActivityStartTime,
				OriginalDataSourceId = agentState.SourceId,
				PersonId = agentState.PersonId,
				PlatformTypeId = agentState.PlatformTypeId,
				ReceivedTime = agentState.ReceivedTime,
				StaffingEffect = agentState.StaffingEffect,
				Adherence = (int?) agentState.Adherence,
				StateCode = agentState.StateCode,
				StateId = agentState.StateGroupId,
				StateStart = agentState.AlarmTypeStartTime
			};
		}
	}
}