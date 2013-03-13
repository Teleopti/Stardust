﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentHandler
	{
		RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId);
		IActualAgentState GetState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
		                           DateTime timestamp,
		                           TimeSpan timeInState);

		IActualAgentState CheckSchedule(Guid personId, Guid businessUnitId, DateTime timestamp);
	}

	public class ActualAgentHandler : IActualAgentHandler
	{
		private readonly IActualAgentDataHandler _actualAgentDataHandler;
		private static readonly ConcurrentDictionary<Guid, IActualAgentState> BatchedAgents = new ConcurrentDictionary<Guid, IActualAgentState>();
		private static readonly object LockObject = new object();
		private static DateTime _lastSave = DateTime.UtcNow;

		public ActualAgentHandler(IActualAgentDataHandler actualAgentDataHandler)
		{
			_actualAgentDataHandler = actualAgentDataHandler;
		}

		public IActualAgentState CheckSchedule(Guid personId, Guid businessUnitId, DateTime timestamp)
		{
			var platformId = Guid.Empty;
			var stateCode = "Unknown";
			var scheduleLayers = _actualAgentDataHandler.CurrentLayerAndNext(timestamp, personId);
			var previousState = _actualAgentDataHandler.LoadOldState(personId);

			if (previousState == null)
				return CreateAndSaveState(scheduleLayers, null, personId, platformId, stateCode, timestamp, new TimeSpan(0),
				                          businessUnitId);

			if (scheduleLayers[0] != null && haveScheduleChanged(scheduleLayers[0], previousState))
				return null;

			platformId = previousState.PlatformTypeId;
			stateCode = previousState.StateCode;

			return CreateAndSaveState(scheduleLayers, previousState, personId, platformId, stateCode, timestamp, new TimeSpan(0),
			                          businessUnitId);
		}

		public IActualAgentState GetState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode, DateTime timestamp,
			TimeSpan timeInState)
		{
			var scheduleLayers = _actualAgentDataHandler.CurrentLayerAndNext(timestamp, personId);
			var previousState = _actualAgentDataHandler.LoadOldState(personId);
			return CreateAndSaveState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timestamp, timeInState, businessUnitId);
		}

		public IActualAgentState CreateAndSaveState(IList<ScheduleLayer> scheduleLayers, IActualAgentState previousState, Guid personId, Guid platformTypeId,
			string stateCode, DateTime timestamp, TimeSpan timeInState, Guid businessUnitId)
		{
			var scheduleLayer = scheduleLayers[0];
			var nextLayer = scheduleLayers[1];

			var foundAlarm = GetAlarm(platformTypeId, stateCode, scheduleLayer, businessUnitId);

			var newState = new ActualAgentState
			{
				PersonId = personId,
				StateCode = stateCode,
				AlarmStart = timestamp,
				PlatformTypeId = platformTypeId,
				ReceivedTime = timestamp
			};

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
				return null;

			BatchedAgents.AddOrUpdate(personId, newState, (guid, state) => newState);

			var utcNow = DateTime.UtcNow;
			if (utcNow.Subtract(_lastSave) >= new TimeSpan(0, 0, 5))
				lock (LockObject)
					if (utcNow.Subtract(_lastSave) >= new TimeSpan(0, 0, 5))
						saveToDataStore(BatchedAgents.Values);
			
			return newState;
		}

		private void saveToDataStore(IEnumerable<IActualAgentState> states)
		{
			var actualAgentStates = states as List<IActualAgentState> ?? states.ToList();
			_actualAgentDataHandler.AddOrUpdate(actualAgentStates);
			_lastSave = DateTime.UtcNow;

			foreach (var agentState in actualAgentStates)
			{
				IActualAgentState outAgentState;
				if (BatchedAgents.TryGetValue(agentState.PersonId, out outAgentState)
				    && agentState.ReceivedTime >= outAgentState.ReceivedTime)
					BatchedAgents.TryRemove(agentState.PersonId, out outAgentState);
			}
		}

		public RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId)
		{
			var state = resolveStateGroupId(platformTypeId, stateCode, businessUnitId);
		    var activityAlarms = _actualAgentDataHandler.ActivityAlarms();
		    var localPayloadId = payloadId(layer);
		    return activityAlarms.ContainsKey(localPayloadId)
		               ? activityAlarms[localPayloadId].SingleOrDefault(
		                   s => s.StateGroupId == state)
		               : null;
		}

		private static Guid payloadId(ScheduleLayer scheduleLayer)
		{
			return scheduleLayer == null ? Guid.Empty : scheduleLayer.PayloadId;
		}

		private Guid resolveStateGroupId(Guid platformTypeId, string stateCode, Guid businessUnitId)
		{
			List<RtaStateGroupLight> outState;
			if (_actualAgentDataHandler.StateGroups().TryGetValue(stateCode, out outState))
			{
				var foundstate =
					outState.FirstOrDefault(s => s.BusinessUnitId == businessUnitId && s.PlatformTypeId == platformTypeId);
				return foundstate != null ? foundstate.StateGroupId : Guid.Empty;
			}
			return Guid.Empty;
		}

		private static bool haveScheduleChanged(ScheduleLayer layer, IActualAgentState oldState)
		{
			return layer.PayloadId == oldState.ScheduledId && layer.StartDateTime == oldState.StateStart &&
			       layer.EndDateTime == oldState.NextStart;
		}
	}
}