using System;
using System.Collections.Generic;
using System.Linq;
using MbCache.Core;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentAssembler
	{
		protected IAlarmMapper AlarmMapper;
		protected IDatabaseReader DatabaseReader;

		public ActualAgentAssembler(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory)
		{
			DatabaseReader = databaseReader;
			AlarmMapper = new AlarmMapper(databaseReader, databaseWriter, mbCacheFactory);
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId)
		{
			var missingAgents = DatabaseReader.GetMissingAgentStatesFromBatch(batchid, sourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
				where !AlarmMapper.IsAgentLoggedOut(
					a.PersonId,
					a.StateCode,
					a.PlatformTypeId,
					a.BusinessUnitId)
				select a;
			return agentsNotAlreadyLoggedOut;
		}

		public IActualAgentState GetAgentState(
			ScheduleLayer currentLayer,
			ScheduleLayer nextLayer,
			IActualAgentState previousState,
			Guid personId,
			Guid businessUnitId,
			Guid platformTypeId,
			string stateCode,
			DateTime timestamp,
			TimeSpan timeInState,
			DateTime? batchId,
			string originalSourceId)
		{
			if (!batchId.HasValue)
				batchId = previousState.BatchId;
			if (platformTypeId == Guid.Empty)
				platformTypeId = previousState.PlatformTypeId;
			if (stateCode == null)
				stateCode = previousState.StateCode;
			if (originalSourceId == null)
				originalSourceId = previousState.OriginalDataSourceId;

			var activityId = currentLayer != null ? currentLayer.PayloadId : Guid.Empty;

			var newState = new ActualAgentState
			{
				PersonId = personId,
				StateCode = stateCode,
				AlarmStart = timestamp,
				PlatformTypeId = platformTypeId,
				ReceivedTime = timestamp,
				OriginalDataSourceId = originalSourceId,
				BusinessUnitId = businessUnitId
			};

			if (batchId.HasValue)
				newState.BatchId = batchId.Value;

			var state = AlarmMapper.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			newState.StateId = state.StateGroupId;
			newState.State = state.StateGroupName;
			var foundAlarm = AlarmMapper.GetAlarm(activityId, state.StateGroupId, businessUnitId);

			if (foundAlarm != null)
			{
				newState.AlarmName = foundAlarm.Name;
				newState.AlarmId = foundAlarm.AlarmTypeId;
				newState.AlarmStart = newState.AlarmStart.AddTicks(foundAlarm.ThresholdTime);
				newState.Color = foundAlarm.DisplayColor;
				newState.StaffingEffect = foundAlarm.StaffingEffect;
				newState.StateStart = timestamp.Add(timeInState.Negate());
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
	}
}