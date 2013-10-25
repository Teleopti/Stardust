﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MbCache.Core;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public class ActualAgentAssembler : IActualAgentAssembler
	{
		private readonly IDatabaseReader _databaseReader;
        private readonly ICurrentAndNextLayerExtractor _currentAndNextLayerExtractor;
        private readonly IMbCacheFactory _mbCacheFactory;
		private readonly IAlarmMapper _alarmMapper;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IActualAgentAssembler));
		private const string notInBatchStatecode = "CCC Logged out";

		public ActualAgentAssembler(IDatabaseReader databaseReader, ICurrentAndNextLayerExtractor currentAndNextLayerExtractor, IMbCacheFactory mbCacheFactory, IAlarmMapper alarmMapper)
		{
			_databaseReader = databaseReader;
		    _currentAndNextLayerExtractor = currentAndNextLayerExtractor;
		    _mbCacheFactory = mbCacheFactory;
			_alarmMapper = alarmMapper;
		}

		protected IDatabaseReader DatabaseReader
		{
			get { return _databaseReader; }
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchId, string sourceId)
		{
			LoggingSvc.InfoFormat("Getting missing agent states from from batch: {0}, sourceId: {1}", batchId, sourceId);
			var missingAgentStates = DatabaseReader.GetMissingAgentStatesFromBatch(batchId, sourceId);
			if (!missingAgentStates.Any())
			{
				LoggingSvc.Info("Did not find any missing agent states, all were included in batch");
				return new Collection<IActualAgentState>();
			}
			LoggingSvc.InfoFormat("Found {0} missing agents", missingAgentStates.Count);

			var updatedAgents = new List<IActualAgentState>();
			var notLoggedOutAgents = missingAgentStates
				.Where(a => !_alarmMapper.IsAgentLoggedOut(a.PersonId,
														   a.StateCode,
														   a.PlatformTypeId,
														   a.BusinessUnit)).ToList();
			LoggingSvc.InfoFormat("Found {0} agents that are not logged out", notLoggedOutAgents.Count);
			foreach (var agentState in notLoggedOutAgents)
			{
				RtaAlarmLight foundAlarm;
				agentState.State = "";
				agentState.StateCode = notInBatchStatecode;
				agentState.StateStart = batchId;
				agentState.AlarmId = Guid.Empty;
				agentState.AlarmName = "";
				agentState.Color = 0;
				agentState.AlarmStart = batchId;
				agentState.StaffingEffect = 0;
				agentState.ReceivedTime = batchId;
				agentState.BatchId = batchId;
				agentState.OriginalDataSourceId = sourceId;

				var stateGroup = _alarmMapper.GetStateGroup(notInBatchStatecode, Guid.Empty, agentState.BusinessUnit);
				if (stateGroup != null)
				{
					agentState.State = stateGroup.StateGroupName;
					foundAlarm = _alarmMapper.GetAlarm(agentState.ScheduledId, stateGroup.StateGroupId, agentState.BusinessUnit);
				}
				else
					foundAlarm = _alarmMapper.GetAlarm(agentState.ScheduledId, Guid.Empty, agentState.BusinessUnit);

				if (foundAlarm != null)
				{
					if (agentState.StateId == foundAlarm.StateGroupId)
					{
						LoggingSvc.DebugFormat("Agent {0} is already in state {1}", agentState.PersonId, agentState.StateId);
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
			LoggingSvc.InfoFormat("{0} agent states have changed", updatedAgents.Count);
			return updatedAgents;
		}

		public IActualAgentState GetAgentStateForScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			var platformId = Guid.Empty;
			var stateCode = string.Empty;
			var originalSourceId = string.Empty;

			LoggingSvc.DebugFormat("Getting readmodel for person: {0}", personId);
			var readModelLayers = DatabaseReader.GetReadModel(personId);
			if (readModelLayers.Any())
				LoggingSvc.DebugFormat("Found {0} layers", readModelLayers.Count);
			else
				LoggingSvc.DebugFormat("No readmodel found for Person: {0}", personId);

			var scheduleLayers = _currentAndNextLayerExtractor.CurrentLayerAndNext(timestamp, readModelLayers);
			var previousState = DatabaseReader.LoadOldState(personId);

			if (previousState == null)
				return buildAgentState(scheduleLayers, null, personId, platformId, stateCode, timestamp, new TimeSpan(0),
									   businessUnitId, null, originalSourceId);

			if (isScheduleSame(scheduleLayers, previousState))
			{
				LoggingSvc.InfoFormat("Schedule have not changed for person {0}, will not save or send state", personId);
				return null;
			}

			platformId = previousState.PlatformTypeId;
			stateCode = previousState.StateCode;
			originalSourceId = previousState.OriginalDataSourceId;
			var batchId = previousState.BatchId;

			return buildAgentState(scheduleLayers, previousState, personId, platformId, stateCode, timestamp, new TimeSpan(0),
								   businessUnitId, batchId, originalSourceId);
		}

        private static bool isScheduleSame(Tuple<ScheduleLayer, ScheduleLayer> layers, IActualAgentState oldState)
		{
			// It might seem strange to check both current.EndDateTime and next.StartDateTime against oldState
			// But either they are the same or some layer will be null.
			var currentSame = layers.Item1 != null
										? layers.Item1.PayloadId == oldState.ScheduledId &&
										  layers.Item1.EndDateTime == oldState.NextStart
										: oldState.ScheduledId == Guid.Empty;

			var nextSame = layers.Item2 != null
										 ? layers.Item2.PayloadId == oldState.ScheduledNextId &&
										   layers.Item2.StartDateTime == oldState.NextStart
										 : oldState.ScheduledNextId == Guid.Empty;

			return currentSame && nextSame;
		}

		public IActualAgentState GetAgentState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
											   DateTime timestamp,
											   TimeSpan timeInState, DateTime? batchId, string originalSourceId)
		{
			LoggingSvc.DebugFormat("Getting readmodel for person: {0}", personId);
			var readModelLayers = DatabaseReader.GetReadModel(personId);
			if (readModelLayers.Any())
				LoggingSvc.DebugFormat("Found {0} layers", readModelLayers.Count);
			else
				LoggingSvc.DebugFormat("No readmodel found for Person: {0}", personId);

			var scheduleLayers = _currentAndNextLayerExtractor.CurrentLayerAndNext(timestamp, readModelLayers);
			var previousState = DatabaseReader.LoadOldState(personId);
			return buildAgentState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timestamp, timeInState,
								   businessUnitId, batchId, originalSourceId);
		}


        private IActualAgentState buildAgentState(Tuple<ScheduleLayer, ScheduleLayer> currentAndNextScheduleLayers, IActualAgentState previousState,
												  Guid personId, Guid platformTypeId,
												  string stateCode, DateTime timestamp, TimeSpan timeInState,
												  Guid businessUnitId, DateTime? batchId, string originalSourceId)
		{
			RtaAlarmLight foundAlarm;
			LoggingSvc.DebugFormat("Starting to build ActualAgentState for personId: {0}", personId);
			var scheduleLayer = currentAndNextScheduleLayers.Item1;
			var nextLayer = currentAndNextScheduleLayers.Item2;
			var activityId = scheduleLayer != null ? scheduleLayer.PayloadId : Guid.Empty;

			var newState = new ActualAgentState
				{
					PersonId = personId,
					StateCode = stateCode,
					AlarmStart = timestamp,
					PlatformTypeId = platformTypeId,
					ReceivedTime = timestamp,
					StateStart = DateTime.UtcNow,
					OriginalDataSourceId = originalSourceId,
					BusinessUnit = businessUnitId
				};

			if (batchId.HasValue)
				newState.BatchId = batchId.Value;

			var state = _alarmMapper.GetStateGroup(stateCode, platformTypeId, businessUnitId);
			if (state != null)
			{
				newState.StateId = state.StateGroupId;
				newState.State = state.StateGroupName;
				foundAlarm = _alarmMapper.GetAlarm(activityId, state.StateGroupId, businessUnitId);
			}
			else
				foundAlarm = _alarmMapper.GetAlarm(activityId, Guid.Empty, businessUnitId);

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

			if (previousState != null && previousState.AlarmId == newState.AlarmId)
				newState.StateStart = previousState.StateStart;

			if (scheduleLayer != null)
			{
				newState.Scheduled = scheduleLayer.Name;
				newState.ScheduledId = scheduleLayer.PayloadId;
				newState.NextStart = scheduleLayer.EndDateTime;
			}

			if (nextLayer != null)
			{
				newState.ScheduledNext = nextLayer.Name;
				newState.NextStart = nextLayer.StartDateTime;
				newState.ScheduledNextId = nextLayer.PayloadId;
				newState.NextStart = nextLayer.StartDateTime;
			}

			//if same don't send it, but should be saved to db to keep batch intact
			if (previousState != null && newState.Equals(previousState))
			{
				LoggingSvc.InfoFormat("The new state is equal to the old state for person {0}, will not send change over message broker",
									  newState.PersonId);
				newState.SendOverMessageBroker = false;
			}
			else
				newState.SendOverMessageBroker = true;

			return newState;
		}

		public void InvalidateReadModelCache(Guid personId)
		{
			LoggingSvc.DebugFormat("Clearing ReadModel cache for Person: {0}", personId);
			_mbCacheFactory.Invalidate(DatabaseReader, x => x.GetReadModel(personId), true);
		}
	}
}