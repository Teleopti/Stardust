using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public class AgentStateAssembler
	{
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

		public AgentState MakeCurrentState(StateInfo info)
		{
			var agentState = new AgentState
			{
				PersonId = info.PersonId,
				BatchId = info.BatchId,
				PlatformTypeId = info.PlatformTypeId,
				SourceId = info.SourceId,
				ReceivedTime = info.CurrentTime,
				StateCode = info.StateCode,
				StateGroupId = info.StateGroupId,
				ActivityId = info.Schedule.CurrentActivityId(),
				AlarmTypeId = info.AlarmTypeId,
				AlarmTypeStartTime = info.AlarmTypeStartTime,
				NextActivityId = info.Schedule.NextActivityId(),
				NextActivityStartTime = info.Schedule.NextActivityStartTime(),
				StaffingEffect = info.StaffingEffect,
				Adherence = info.AdherenceState2
			};
			return agentState;
		}
	}
}