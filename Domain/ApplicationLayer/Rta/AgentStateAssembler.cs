using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
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
				StaffingEffect = fromStorage.StaffingEffect
			};
		}

		public AgentState MakePreviousState(Guid personId, AgentStateReadModel fromStorage)
		{
			if (fromStorage == null)
				return new AgentState
				{
					PersonId = personId,
					StateGroupId = Guid.NewGuid()
				};
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
				StaffingEffect = fromStorage.StaffingEffect
			};
		}

		public AgentState MakeCurrentState(ScheduleInfo scheduleInfo, ExternalUserStateInputModel input, PersonOrganizationData person, AgentState previous, DateTime currentTime)
		{
			var platformTypeId = string.IsNullOrEmpty(input.PlatformTypeId) ?  previous.PlatformTypeId : input.ParsedPlatformTypeId();
			var stateCode = input.StateCode ?? previous.StateCode;
			//var state = _alarmFinder.StateCodeInfoFor(stateCode, input.StateDescription, platformTypeId, person.BusinessUnitId);
			var state = _stateMapper.StateFor(person.BusinessUnitId, platformTypeId, stateCode, input.StateDescription);
			//var alarm = _alarmFinder.GetAlarm(scheduleInfo.CurrentActivityId(), stateGroup.StateGroupId, person.BusinessUnitId) ?? new AlarmMappingInfo();
			var alarm = _stateMapper.AlarmFor(person.BusinessUnitId, stateCode, scheduleInfo.CurrentActivityId()) ?? new AlarmMapping();
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
				StaffingEffect = alarm.StaffingEffect
			};
			agentState.UseAssembleMethod(() => new AgentStateReadModel
			{
				AlarmId = alarm.AlarmTypeId,
				AlarmName = alarm.AlarmName,
				BatchId = agentState.BatchId,
				AlarmStart = currentTime.AddTicks(alarm.ThresholdTime),
				BusinessUnitId = person.BusinessUnitId,
				SiteId = person.SiteId,
				TeamId = person.TeamId,
				Color = alarm.DisplayColor,
				NextStart = agentState.NextActivityStartTime,
				OriginalDataSourceId = agentState.SourceId,
				PersonId = agentState.PersonId,
				PlatformTypeId = agentState.PlatformTypeId,
				ReceivedTime = agentState.ReceivedTime,
				Scheduled = scheduleInfo.CurrentActivityName(),
				ScheduledId = scheduleInfo.CurrentActivityId(),
				ScheduledNext = scheduleInfo.NextActivityName(),
				ScheduledNextId = scheduleInfo.NextActivityId(),
				StaffingEffect = agentState.StaffingEffect,
				State = state.StateGroupName,
				StateCode = agentState.StateCode,
				StateId = agentState.StateGroupId,
				StateStart = agentState.AlarmTypeStartTime
			});
			return agentState;
		}
	}
}