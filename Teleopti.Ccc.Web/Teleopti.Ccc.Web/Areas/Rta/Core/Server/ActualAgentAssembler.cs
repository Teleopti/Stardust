using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentAssembler
	{
		private static readonly ILog loggingSvc = LogManager.GetLogger(typeof(ActualAgentAssembler));
		private const string notInBatchStatecode = "CCC Logged out";

		protected IAlarmMapper AlarmMapper;
		protected IDatabaseReader DatabaseReader;

		public ActualAgentAssembler(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory)
		{
			DatabaseReader = databaseReader;
			AlarmMapper = new AlarmMapper(databaseReader, databaseWriter, mbCacheFactory);
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchId, string sourceId)
		{
			loggingSvc.InfoFormat("Getting missing agent states from from batch: {0}, sourceId: {1}", batchId, sourceId);
			var missingAgentStates = DatabaseReader.GetMissingAgentStatesFromBatch(batchId, sourceId).ToList();
			if (!missingAgentStates.Any())
			{
				loggingSvc.Info("Did not find any missing agent states, all were included in batch");
				return new Collection<IActualAgentState>();
			}
			loggingSvc.InfoFormat("Found {0} missing agents", missingAgentStates.Count());

			var updatedAgents = new List<IActualAgentState>();
			var notLoggedOutAgents = missingAgentStates
				.Where(a => !AlarmMapper.IsAgentLoggedOut(a.PersonId,
														   a.StateCode,
														   a.PlatformTypeId,
														   a.BusinessUnit)).ToList();
			loggingSvc.InfoFormat("Found {0} agents that are not logged out", notLoggedOutAgents.Count);
			foreach (var agentState in notLoggedOutAgents)
			{
				RtaAlarmLight foundAlarm;
				agentState.State = "";
				agentState.StateCode = notInBatchStatecode;
				agentState.StateStart = batchId;
				agentState.AlarmId = Guid.Empty;
				agentState.AlarmName = "";
				agentState.AlarmStart = batchId;
				agentState.StaffingEffect = 0;
				agentState.ReceivedTime = batchId;
				agentState.BatchId = batchId;
				agentState.OriginalDataSourceId = sourceId;

				var stateGroup = AlarmMapper.GetStateGroup(notInBatchStatecode, Guid.Empty, agentState.BusinessUnit);
				if (stateGroup != null)
				{
					agentState.State = stateGroup.StateGroupName;
					foundAlarm = AlarmMapper.GetAlarm(agentState.ScheduledId, stateGroup.StateGroupId, agentState.BusinessUnit);
				}
				else
					foundAlarm = AlarmMapper.GetAlarm(agentState.ScheduledId, Guid.Empty, agentState.BusinessUnit);

				if (foundAlarm != null)
				{
					if (agentState.StateId == foundAlarm.StateGroupId)
					{
						loggingSvc.DebugFormat("Agent {0} is already in state {1}", agentState.PersonId, agentState.StateId);
						continue;
					}

					agentState.State = foundAlarm.StateGroupName;
					agentState.StateId = foundAlarm.StateGroupId;
					agentState.StateStart = batchId;
					agentState.AlarmName = foundAlarm.Name;
					agentState.AlarmId = foundAlarm.AlarmTypeId;
					agentState.Color = foundAlarm.DisplayColor;
					agentState.AlarmStart = batchId.AddTicks(foundAlarm.ThresholdTime);
					agentState.StaffingEffect = foundAlarm.StaffingEffect;
					agentState.ReceivedTime = batchId;
					agentState.BatchId = batchId;
					agentState.OriginalDataSourceId = sourceId;
				}
				else if (stateGroup != null)
					agentState.StateId = stateGroup.StateGroupId;
				updatedAgents.Add(agentState);
			}
			loggingSvc.InfoFormat("{0} agent states have changed", updatedAgents.Count);
			return updatedAgents;
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
			loggingSvc.DebugFormat("Starting to build ActualAgentState for personId: {0}", personId);

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
				BusinessUnit = businessUnitId
			};

			if (batchId.HasValue)
				newState.BatchId = batchId.Value;

			RtaAlarmLight foundAlarm;
			var state = AlarmMapper.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			if (state != null)
			{
				newState.StateId = state.StateGroupId;
				newState.State = state.StateGroupName;
				foundAlarm = AlarmMapper.GetAlarm(activityId, state.StateGroupId, businessUnitId);
			}
			else
				foundAlarm = AlarmMapper.GetAlarm(activityId, Guid.Empty, businessUnitId);

			if (foundAlarm != null)
			{
				newState.AlarmName = foundAlarm.Name;
				newState.AlarmId = foundAlarm.AlarmTypeId;
				newState.AlarmStart = newState.AlarmStart.AddTicks(foundAlarm.ThresholdTime);
				newState.Color = foundAlarm.DisplayColor;
				newState.StaffingEffect = foundAlarm.StaffingEffect;
				newState.State = foundAlarm.StateGroupName;
				newState.StateId = foundAlarm.StateGroupId;
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