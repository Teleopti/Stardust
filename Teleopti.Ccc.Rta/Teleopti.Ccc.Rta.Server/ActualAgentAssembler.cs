using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using MbCache.Core;
using Teleopti.Ccc.Domain.Collection;
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
		RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid state);

		IActualAgentState GetAndSaveState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
		                                  DateTime timestamp,
		                                  TimeSpan timeInState,
		                                  DateTime? batchId, string originalSourceId);

		IActualAgentState CheckSchedule(Guid personId, Guid businessUnitId, DateTime timestamp);
		void FlushMemoryToDatabase();
		IEnumerable<IActualAgentState> GetAndSaveStateForMissingAgent(DateTime batchId, string sourceId);
	}

	public class ActualAgentAssembler : IActualAgentAssembler
	{
		private readonly IDatabaseHandler _databaseHandler;
		private readonly IMbCacheFactory _mbCacheFactory;
		private static readonly ConcurrentDictionary<Guid, IActualAgentState> BatchedAgents = new ConcurrentDictionary<Guid, IActualAgentState>();
		private static readonly object LockObject = new object();
		private static readonly ILog LoggingSvc = LogManager.GetLogger(typeof(IActualAgentAssembler));
		private static DateTime _lastSave = DateTime.UtcNow;

		public ActualAgentAssembler(IDatabaseHandler databaseHandler, IMbCacheFactory mbCacheFactory)
		{
			_databaseHandler = databaseHandler;
			_mbCacheFactory = mbCacheFactory;
		}

		protected IDatabaseHandler DatabaseHandler
		{
			get { return _databaseHandler; }
		}

		public IActualAgentState CheckSchedule(Guid personId, Guid businessUnitId, DateTime timestamp)
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
				return createAndSaveState(scheduleLayers, null, personId, platformId, stateCode, timestamp, new TimeSpan(0),
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

			return createAndSaveState(scheduleLayers, previousState, personId, platformId, stateCode, timestamp, new TimeSpan(0),
									  businessUnitId, batchId, originalSourceId);
		}

		public IEnumerable<IActualAgentState> GetAndSaveStateForMissingAgent(DateTime batchId, string sourceId)
		{
			LoggingSvc.InfoFormat("Flushing cached agent states to database");
			FlushMemoryToDatabase();

			LoggingSvc.InfoFormat("Getting missing agent states from from batch: {0}, sourceId: {1}", batchId, sourceId);
			var missingAgentStates = DatabaseHandler.GetMissingAgentStatesFromBatch(batchId, sourceId);
			if (!missingAgentStates.Any())
			{
				LoggingSvc.InfoFormat("Did not find any missing agent states, all were included in batch");
				return new Collection<IActualAgentState>();
			}
			LoggingSvc.InfoFormat("Found {0} missing agents", missingAgentStates.Count);

			var activityAlarms = DatabaseHandler.ActivityAlarms();
			var agentsToSendOverMessageBroker = new List<IActualAgentState>();
			foreach (var agentState in missingAgentStates)
			{
				var state = agentState;
				state.State = "";
				state.StateCode = "LOGGED-OFF";
				state.StateId = Guid.Empty;
				state.StateStart = batchId;
				state.AlarmId = Guid.Empty;
				state.AlarmName = "";
				state.Color = 0;
				state.AlarmStart = batchId;
				state.StaffingEffect = 0;
				state.ReceivedTime = batchId;
				state.BatchId = batchId;
				state.OriginalDataSourceId = sourceId;

				List<RtaAlarmLight> alarmList;
				if (!activityAlarms.TryGetValue(state.ScheduledId, out alarmList))
					LoggingSvc.InfoFormat("Could not find any alarms connected to this activity id: {0}", state.ScheduledId);

				else
				{
					var loggedOutState = alarmList.FirstOrDefault(a => a.IsLogOutState);
					if (loggedOutState == null)
						LoggingSvc.InfoFormat("Could not find any alarm that is set to logged out");

					else
					{
						if (state.StateId == loggedOutState.StateGroupId)
						{
							LoggingSvc.InfoFormat("Agent {0} is already in logged out state {1}", agentState.PersonId, state.StateId);
							continue;
						}

						state.StateCode = loggedOutState.StateGroupName;
						state.StateId = loggedOutState.StateGroupId;
						state.StateStart = batchId;
						state.AlarmName = loggedOutState.Name;
						state.AlarmId = loggedOutState.AlarmTypeId;
						state.Color = loggedOutState.DisplayColor;
						state.AlarmStart = batchId.AddTicks(loggedOutState.ThresholdTime);
						state.StaffingEffect = loggedOutState.StaffingEffect;
						state.ReceivedTime = batchId;
						state.BatchId = batchId;
						state.OriginalDataSourceId = sourceId;
					}
				}

				BatchedAgents.AddOrUpdate(state.PersonId, state, (guid, oldState) => state);
				agentsToSendOverMessageBroker.Add(state);
			}
			LoggingSvc.InfoFormat("Saving {0} agents to database", BatchedAgents.Count);
			lock (LockObject)
				saveToDataStore(BatchedAgents.Values);

			LoggingSvc.InfoFormat("Found {0} agents to send over message broker", agentsToSendOverMessageBroker.Count);
			return agentsToSendOverMessageBroker;
		}

		public IActualAgentState GetAndSaveState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode, DateTime timestamp,
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
			return createAndSaveState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timestamp, timeInState, businessUnitId, batchId, originalSourceId);
		}


		private IActualAgentState createAndSaveState(IList<ScheduleLayer> scheduleLayers, IActualAgentState previousState, Guid personId, Guid platformTypeId,
			string stateCode, DateTime timestamp, TimeSpan timeInState, Guid businessUnitId, DateTime? batchId, string originalSourceId)
		{
			RtaAlarmLight foundAlarm;
			var scheduleLayer = scheduleLayers.FirstOrDefault();
			var nextLayer = scheduleLayers.LastOrDefault();
			var state = resolveStateGroupId(platformTypeId, stateCode, businessUnitId);

			LoggingSvc.InfoFormat("Starting to build ActualAgentState for personId: {0}", personId);

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

			if (state != null)
			{
				newState.State = state.StateGroupName;
				foundAlarm = GetAlarm(platformTypeId, stateCode, scheduleLayer, state.StateGroupId);
			}

			else
				foundAlarm = GetAlarm(platformTypeId, stateCode, scheduleLayer, Guid.Empty);

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
				LoggingSvc.InfoFormat("The new state is equal to the old state for person {0}, will not send or save", newState.PersonId);
				return null;
			}

			LoggingSvc.InfoFormat("ActualAgentState cache - Adding/updating state: {0}", newState);
			BatchedAgents.AddOrUpdate(personId, newState, (guid, oldState) => newState);

			var utcNow = DateTime.UtcNow;
			if (utcNow.Subtract(_lastSave) >= new TimeSpan(0, 0, 5))
				lock (LockObject)
					if (utcNow.Subtract(_lastSave) >= new TimeSpan(0, 0, 5))
						saveToDataStore(BatchedAgents.Values);
			
			return newState;
		}
		
		public void FlushMemoryToDatabase()
		{
			lock (LockObject)
				saveToDataStore(BatchedAgents.Values);
		}
		
		private void saveToDataStore(IEnumerable<IActualAgentState> states)
		{
			var actualAgentStates = states as List<IActualAgentState> ?? states.ToList();
			LoggingSvc.InfoFormat("Saving {0} states to db.", actualAgentStates.Count);

			DatabaseHandler.AddOrUpdate(actualAgentStates);
			
			_lastSave = DateTime.UtcNow;
			foreach (var agentState in actualAgentStates)
			{
				IActualAgentState outAgentState;
				if (BatchedAgents.TryGetValue(agentState.PersonId, out outAgentState)
				    && agentState.ReceivedTime >= outAgentState.ReceivedTime)
				{
					LoggingSvc.InfoFormat("ActualAgentState cache - Removing state: {0}", outAgentState);
					BatchedAgents.TryRemove(agentState.PersonId, out outAgentState);
				}
			}
		}

		public RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid state)
		{
			LoggingSvc.InfoFormat("Getting alarm for PlatformId: {0}, StateCode: {1}", platformTypeId, stateCode);
			LoggingSvc.Info("Loading activity alarms.");
		    var activityAlarms = DatabaseHandler.ActivityAlarms();
		    var localPayloadId = payloadId(layer);
			List<RtaAlarmLight> list;

			if (activityAlarms.TryGetValue(localPayloadId, out list))
			{
				var alarm = list.SingleOrDefault(s => s.StateGroupId == state);
				if (alarm != null)
					LoggingSvc.InfoFormat("Found alarm: {1}, alarmId: {0}", alarm.AlarmTypeId, alarm.Name);
				else
					LoggingSvc.InfoFormat("Could not find alarm (no matching stategroupid) for PlatformId: {0}, StateCode: {1}", platformTypeId, stateCode);
				return alarm;
			}
			
			LoggingSvc.InfoFormat("Could not find alarm (no matching schedulelayer payloadId)  for PlatformId: {0}, StateCode: {1}", platformTypeId, stateCode);
			return null;
		}

		private static Guid payloadId(ScheduleLayer scheduleLayer)
		{
			return scheduleLayer == null ? Guid.Empty : scheduleLayer.PayloadId;
		}

		private RtaStateGroupLight resolveStateGroupId(Guid platformTypeId, string stateCode, Guid businessUnitId)
		{
			LoggingSvc.Info("Loading stategroups");
			List<RtaStateGroupLight> outState;
			if (DatabaseHandler.StateGroups().TryGetValue(stateCode, out outState))
			{
				var foundstate =
					outState.FirstOrDefault(s => s.BusinessUnitId == businessUnitId && s.PlatformTypeId == platformTypeId);
				if (foundstate != null)
					return foundstate;
			}
			else if (!string.IsNullOrEmpty(stateCode))
			{
				LoggingSvc.InfoFormat("Could not find state: {0}, on platform {1} adding it to database", stateCode, platformTypeId);
				DatabaseHandler.AddNewRtaState(stateCode, platformTypeId);
				LoggingSvc.Info("Clearing cache for StateGroups");
				invalidateStateGroupCache();
			}

			LoggingSvc.WarnFormat("Could not find StateGroup for PlatformId: {0}, StateCode: {1}, BU: {2}", platformTypeId, stateCode, businessUnitId);
			return null;
		}

		private void invalidateStateGroupCache()
		{
			_mbCacheFactory.Invalidate(DatabaseHandler, x => x.StateGroups(), false);
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