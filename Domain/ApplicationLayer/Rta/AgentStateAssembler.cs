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

		public AgentState MakePreviousState(Guid personId, IActualAgentState state)
		{
			if (state == null)
				return new AgentState()
				{
					PersonId = personId,
					StateGroupId = Guid.NewGuid()
				};
			return new AgentState
			{
				PersonId = state.PersonId,
				BatchId = state.BatchId,
				PlatformTypeId = state.PlatformTypeId,
				SourceId = state.OriginalDataSourceId,
				ReceivedTime = state.ReceivedTime,
				StateCode = state.StateCode,
				StateGroupId = state.StateId,
				ActivityId = state.ScheduledId,
				NextActivityId = state.ScheduledNextId,
				NextActivityStartTime = state.NextStart,
				AlarmTypeId = state.AlarmId,
				AlarmTypeStartTime = state.StateStart,
				StaffingEffect = state.StaffingEffect
			};
		}

		public AgentState MakeCurrentState(ScheduleInfo scheduleInfo, ExternalUserStateInputModel input, PersonOrganizationData person, AgentState previous, DateTime currentTime)
		{
			var platformTypeId = string.IsNullOrEmpty(input.PlatformTypeId) ?  previous.PlatformTypeId : input.ParsedPlatformTypeId();
			var stateCode = input.StateCode ?? previous.StateCode;
			var stateGroup = _alarmFinder.GetStateGroup(stateCode, platformTypeId, person.BusinessUnitId);
			var alarm = _alarmFinder.GetAlarm(scheduleInfo.CurrentActivityId(), stateGroup.StateGroupId, person.BusinessUnitId) ?? new RtaAlarmLight();
			var rtaAgentState = new AgentState
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
			rtaAgentState.UseAssembleMethod(() => new ActualAgentState
			{
				AlarmId = alarm.AlarmTypeId,
				AlarmName = alarm.Name,
				BatchId = rtaAgentState.BatchId,
				AlarmStart = currentTime.AddTicks(alarm.ThresholdTime),
				BusinessUnitId = person.BusinessUnitId,
				Color = alarm.DisplayColor,
				NextStart = rtaAgentState.NextActivityStartTime,
				OriginalDataSourceId = rtaAgentState.SourceId,
				PersonId = rtaAgentState.PersonId,
				PlatformTypeId = rtaAgentState.PlatformTypeId,
				ReceivedTime = rtaAgentState.ReceivedTime,
				Scheduled = scheduleInfo.CurrentActivityName(),
				ScheduledId = scheduleInfo.CurrentActivityId(),
				ScheduledNext = scheduleInfo.NextActivityName(),
				ScheduledNextId = scheduleInfo.NextActivityId(),
				StaffingEffect = rtaAgentState.StaffingEffect,
				State = stateGroup.StateGroupName,
				StateCode = rtaAgentState.StateCode,
				StateId = rtaAgentState.StateGroupId,
				StateStart = rtaAgentState.AlarmTypeStartTime
			});
			return rtaAgentState;
		}
	}
}