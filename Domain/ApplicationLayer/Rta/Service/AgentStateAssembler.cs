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

		public AgentState MakeCurrentState(StateInfo info, ExternalUserStateInputModel input, PersonOrganizationData person, AgentState previous, DateTime currentTime)
		{
			var agentState = new AgentState
			{
				PersonId = info.PersonId,
				BatchId = input.IsSnapshot ? input.BatchId : previous.BatchId,
				PlatformTypeId = info.PlatformTypeId,
				SourceId = input.SourceId ?? previous.SourceId,
				ReceivedTime = currentTime,
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
			agentState.UseAssembleMethod(() =>
			{
				var model = ReadModelFromState(agentState);
				model.AlarmId = info.AlarmTypeId;
				model.AlarmName = info.AlarmName;
				model.AlarmStart = currentTime.AddTicks(info.AlarmThresholdTime);
				model.BusinessUnitId = person.BusinessUnitId;
				model.SiteId = person.SiteId;
				model.TeamId = person.TeamId;
				model.Color = info.AlarmDisplayColor;
				model.Scheduled = info.Schedule.CurrentActivityName();
				model.ScheduledId = info.Schedule.CurrentActivityId();
				model.ScheduledNext = info.Schedule.NextActivityName();
				model.ScheduledNextId = info.Schedule.NextActivityId();
				model.State = info.StateGroupName;
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