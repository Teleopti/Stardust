using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MbCache.Core;
using Teleopti.Interfaces.Domain;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IRtaDataHandlerCache
	{
		void InvalidateReadModelCache(Guid personId);
	}

	public interface IActualAgentAssembler : IRtaDataHandlerCache
	{
		IActualAgentState GetAgentState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
		                                DateTime timestamp,
		                                TimeSpan timeInState,
		                                DateTime? batchId, string originalSourceId);

		IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchId, string sourceId);
		IActualAgentState GetAgentStateForScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp);
	}

	public class ActualAgentAssembler : IActualAgentAssembler
	{
		private readonly IDatabaseHandler _databaseHandler;
		private readonly IMbCacheFactory _mbCacheFactory;
		private readonly IAlarmMapper _alarmMapper;
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof (IActualAgentAssembler));
		private const string notInBatchStatecode = "CCC Logged out";

		public ActualAgentAssembler(IDatabaseHandler databaseHandler, IMbCacheFactory mbCacheFactory, IAlarmMapper alarmMapper)
		{
			_databaseHandler = databaseHandler;
			_mbCacheFactory = mbCacheFactory;
			_alarmMapper = alarmMapper;
		}

		protected IDatabaseHandler DatabaseHandler
		{
			get { return _databaseHandler; }
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchId, string sourceId)
		{
			LoggingSvc.InfoFormat("Getting missing agent states from from batch: {0}, sourceId: {1}", batchId, sourceId);
			var missingAgentStates = DatabaseHandler.GetMissingAgentStatesFromBatch(batchId, sourceId);
			if (!missingAgentStates.Any())
			{
				LoggingSvc.InfoFormat("Did not find any missing agent states, all were included in batch");
				return new Collection<IActualAgentState>();
			}
			LoggingSvc.InfoFormat("Found {0} missing agents", missingAgentStates.Count);

			var updatedAgents = new List<IActualAgentState>();
			foreach (var agentState in missingAgentStates
				.Where(agentState => !_alarmMapper.IsAgentLoggedOut(agentState.PersonId,
				                                                    agentState.StateCode,
				                                                    agentState.PlatformTypeId,
				                                                    agentState.BusinessUnit)))
			{
				RtaAlarmLight foundAlarm;
				var state = agentState;
				state.State = "";
				state.StateCode = notInBatchStatecode;
				state.StateStart = batchId;
				state.AlarmId = Guid.Empty;
				state.AlarmName = "";
				state.Color = 0;
				state.AlarmStart = batchId;
				state.StaffingEffect = 0;
				state.ReceivedTime = batchId;
				state.BatchId = batchId;
				state.OriginalDataSourceId = sourceId;

				var stateGroup = _alarmMapper.GetStateGroup(notInBatchStatecode, Guid.Empty, state.BusinessUnit);
				if (stateGroup != null)
				{
					state.State = stateGroup.StateGroupName;
					foundAlarm = _alarmMapper.GetAlarm(state.ScheduledId, stateGroup.StateGroupId);
				}
				else
					foundAlarm = _alarmMapper.GetAlarm(state.ScheduledId, Guid.Empty);

				if (foundAlarm != null)
				{
					if (state.StateId == foundAlarm.StateGroupId)
					{
						LoggingSvc.InfoFormat("Agent {0} is already in logged out state {1}", agentState.PersonId, state.StateId);
						continue;
					}

					state.State = foundAlarm.StateGroupName;
					state.StateId = foundAlarm.StateGroupId;
					state.StateStart = batchId;
					state.AlarmName = foundAlarm.Name;
					state.AlarmId = foundAlarm.AlarmTypeId;
					state.Color = foundAlarm.DisplayColor;
					state.AlarmStart = batchId.AddTicks(foundAlarm.ThresholdTime);
					state.StaffingEffect = foundAlarm.StaffingEffect;
					state.ReceivedTime = batchId;
					state.BatchId = batchId;
					state.OriginalDataSourceId = sourceId;
				}
				updatedAgents.Add(state);
			}
			LoggingSvc.InfoFormat("Found {0} agents to send over message broker", updatedAgents.Count);
			return updatedAgents;
		}

		public IActualAgentState GetAgentStateForScheduleUpdate(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			var platformId = Guid.Empty;
			var stateCode = string.Empty;
			var originalSourceId = string.Empty;

			LoggingSvc.InfoFormat("Getting readmodel for person: {0}", personId);
			var readModelLayers = DatabaseHandler.GetReadModel(personId);
			if (readModelLayers.Any())
				LoggingSvc.InfoFormat("Found {0} layers", readModelLayers.Count);
			else
				LoggingSvc.InfoFormat("No readmodel found for Person: {0}", personId);

			var scheduleLayers = DatabaseHandler.CurrentLayerAndNext(timestamp, readModelLayers);
			var previousState = DatabaseHandler.LoadOldState(personId);

			if (previousState == null)
				return buildAgentState(scheduleLayers, null, personId, platformId, stateCode, timestamp, new TimeSpan(0),
									   businessUnitId, null, originalSourceId);

			if (scheduleLayers[0] != null && haveScheduleChanged(scheduleLayers[0], previousState))
			{
				LoggingSvc.InfoFormat("State have not changed for person {0}, will not save or send state", personId);
				return null;
			}

			platformId = previousState.PlatformTypeId;
			stateCode = previousState.StateCode;
			originalSourceId = previousState.OriginalDataSourceId;
			var batchId = previousState.BatchId;

			return buildAgentState(scheduleLayers, previousState, personId, platformId, stateCode, timestamp, new TimeSpan(0),
								   businessUnitId, batchId, originalSourceId);
		}

		public IActualAgentState GetAgentState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
		                                       DateTime timestamp,
		                                       TimeSpan timeInState, DateTime? batchId, string originalSourceId)
		{
			LoggingSvc.InfoFormat("Getting readmodel for person: {0}", personId);
			var readModelLayers = DatabaseHandler.GetReadModel(personId);
			if (readModelLayers.Any())
				LoggingSvc.InfoFormat("Found {0} layers", readModelLayers.Count);
			else
				LoggingSvc.InfoFormat("No readmodel found for Person: {0}", personId);

			var scheduleLayers = DatabaseHandler.CurrentLayerAndNext(timestamp, readModelLayers);
			var previousState = DatabaseHandler.LoadOldState(personId);
			return buildAgentState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timestamp, timeInState,
			                       businessUnitId, batchId, originalSourceId);
		}


		private IActualAgentState buildAgentState(IList<ScheduleLayer> scheduleLayers, IActualAgentState previousState,
		                                          Guid personId, Guid platformTypeId,
		                                          string stateCode, DateTime timestamp, TimeSpan timeInState,
		                                          Guid businessUnitId, DateTime? batchId, string originalSourceId)
		{
			RtaAlarmLight foundAlarm;
			LoggingSvc.InfoFormat("Starting to build ActualAgentState for personId: {0}", personId);
			var scheduleLayer = scheduleLayers.FirstOrDefault();
			var nextLayer = scheduleLayers.LastOrDefault();
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
				foundAlarm = _alarmMapper.GetAlarm(activityId, state.StateGroupId);
			}
			else
				foundAlarm = _alarmMapper.GetAlarm(activityId, Guid.Empty);
			
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

				if (previousState != null && previousState.AlarmId == newState.AlarmId)
					newState.StateStart = previousState.StateStart;
			}

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

			//if same don't send it (could happen often in batchmode)
			if (previousState != null && newState.Equals(previousState))
			{
				LoggingSvc.InfoFormat("The new state is equal to the old state for person {0}, will not send or save",
				                      newState.PersonId);
				return null;
			}

			LoggingSvc.InfoFormat("ActualAgentState cache - Adding/updating state: {0}", newState);

			return newState;
		}
		
		private static bool haveScheduleChanged(ScheduleLayer layer, IActualAgentState oldState)
		{
			return layer.PayloadId == oldState.ScheduledId
			       && layer.StartDateTime == oldState.StateStart
			       && layer.EndDateTime == oldState.NextStart;
		}

		public void InvalidateReadModelCache(Guid personId)
		{
			LoggingSvc.InfoFormat("Clearing ReadModel cache for Person: {0}", personId);
			_mbCacheFactory.Invalidate(DatabaseHandler, x => x.GetReadModel(personId), true);
		}
	}
}