using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateAssembler
	{
		public PreviousAgentState MakeEmpty(Guid personId)
		{
			return new PreviousAgentState
			{
				PersonId = personId,
				StateGroupId = Guid.NewGuid()
			};
		}

		public CurrentAgentState MakeCurrentStateFromPrevious(AgentStateReadModel fromStorage)
		{
			return new CurrentAgentState
			{
				ActivityId = fromStorage.ScheduledId,
				NextActivityId = fromStorage.ScheduledNextId,
				NextActivityStartTime = fromStorage.NextStart,
				AlarmTypeId = fromStorage.AlarmId,
				AlarmTypeStartTime = fromStorage.StateStart,
				StaffingEffect = fromStorage.StaffingEffect,
				Adherence = (AdherenceState?) fromStorage.Adherence
			};
		}

		public PreviousAgentState MakePreviousState(Guid personId, AgentStateReadModel fromStorage)
		{
			if (fromStorage == null)
				return MakeEmpty(personId);
			return new PreviousAgentState
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

		public CurrentAgentState MakeCurrentState(StateInfo info)
		{
			return new CurrentAgentState
			{
				ActivityId = info.Schedule.CurrentActivityId(),
				AlarmTypeId = info.AlarmTypeId,
				AlarmTypeStartTime = info.AlarmTypeStartTime,
				NextActivityId = info.Schedule.NextActivityId(),
				NextActivityStartTime = info.Schedule.NextActivityStartTime(),
				StaffingEffect = info.StaffingEffect,
				Adherence = info.AdherenceState2
			};
		}
	}
}