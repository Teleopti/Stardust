using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public class AgentStateAssembler
	{
		private readonly IAlarmFinder _alarmFinder;

		public AgentStateAssembler(IAlarmFinder alarmFinder)
		{
			_alarmFinder = alarmFinder;
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
			var stateGroup = _alarmFinder.GetStateGroup(stateCode, platformTypeId, person.BusinessUnitId);
			var alarm = _alarmFinder.GetAlarm(scheduleInfo.CurrentActivityId(), stateGroup.StateGroupId, person.BusinessUnitId) ?? new RtaAlarmLight();
			var agentState = new AgentState
			{
				PersonId = person.PersonId,
				BatchId = input.IsSnapshot ? input.BatchId : previous.BatchId,
				PlatformTypeId = platformTypeId,
				SourceId = input.SourceId ?? previous.SourceId,
				ReceivedTime = currentTime,
				StateCode = stateCode,
				StateGroupId = stateGroup.StateGroupId,
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
				AlarmName = alarm.Name,
				BatchId = agentState.BatchId,
				AlarmStart = currentTime.AddTicks(alarm.ThresholdTime),
				BusinessUnitId = person.BusinessUnitId,
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
				State = stateGroup.StateGroupName,
				StateCode = agentState.StateCode,
				StateId = agentState.StateGroupId,
				StateStart = agentState.AlarmTypeStartTime
			});
			return agentState;
		}
	}
}