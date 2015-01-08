using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentAssembler : IActualAgentAssembler
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IAlarmFinder _alarmFinder;

		public ActualAgentAssembler(IDatabaseReader databaseReader, IAlarmFinder alarmFinder)
		{
			_databaseReader = databaseReader;
			_alarmFinder = alarmFinder;
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId)
		{
			var missingAgents = _databaseReader.GetMissingAgentStatesFromBatch(batchid, sourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
				where !IsAgentLoggedOut(
					a.PersonId,
					a.StateCode,
					a.PlatformTypeId,
					a.BusinessUnitId)
				select a;
			return agentsNotAlreadyLoggedOut;
		}

		public IActualAgentState GetAgentState(ExternalUserStateInputModel input, PersonOrganizationData person, ScheduleLayer currentLayer, ScheduleLayer nextLayer, IActualAgentState previousState, DateTime currentTime)
		{
			var batchId = input.IsSnapshot
				? input.BatchId
				: (DateTime?)null;

			var stateCode = input.StateCode;
			var originalSourceId = input.SourceId;
			var personId = person.PersonId;
			var businessUnitId = person.BusinessUnitId;

			if (!batchId.HasValue)
				batchId = previousState.BatchId;
			if (string.IsNullOrEmpty(input.PlatformTypeId))
				input.PlatformTypeId = previousState.PlatformTypeId.ToString();
			if (stateCode == null)
				stateCode = previousState.StateCode;
			if (originalSourceId == null)
				originalSourceId = previousState.OriginalDataSourceId;

			var activityId = currentLayer != null ? currentLayer.PayloadId : Guid.Empty;

			var newState = new ActualAgentState
			{
				PersonId = personId,
				StateCode = stateCode,
				AlarmStart = currentTime,
				PlatformTypeId = input.ParsedPlatformTypeId(),
				ReceivedTime = currentTime,
				OriginalDataSourceId = originalSourceId,
				BusinessUnitId = businessUnitId
			};

			if (batchId.HasValue)
				newState.BatchId = batchId.Value;

			var state = _alarmFinder.GetStateGroup(stateCode, input.ParsedPlatformTypeId(), businessUnitId);
			var foundAlarm = _alarmFinder.GetAlarm(activityId, state.StateGroupId, businessUnitId);

			newState.StateId = state.StateGroupId;
			newState.State = state.StateGroupName;
			if (foundAlarm != null)
			{
				newState.AlarmName = foundAlarm.Name;
				newState.AlarmId = foundAlarm.AlarmTypeId;
				newState.AlarmStart = newState.AlarmStart.AddTicks(foundAlarm.ThresholdTime);
				newState.Color = foundAlarm.DisplayColor;
				newState.StaffingEffect = foundAlarm.StaffingEffect;
				newState.StateStart = currentTime;
			}

			if (previousState.AlarmId == newState.AlarmId)
				newState.StateStart = previousState.StateStart;

			if (currentLayer != null)
			{
				newState.Scheduled = currentLayer.Name;
				newState.ScheduledId = currentLayer.PayloadId;
				newState.NextStart = currentLayer.EndDateTime;
			}

			if (nextLayer != null)
			{
				newState.ScheduledNext = nextLayer.Name;
				newState.NextStart = nextLayer.StartDateTime;
				newState.ScheduledNextId = nextLayer.PayloadId;
				newState.NextStart = nextLayer.StartDateTime;
			}
			return newState;
		}

		public bool IsAgentLoggedOut(Guid personId, string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var state = _alarmFinder.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			return state != null && state.IsLogOutState;
		}

	}
}