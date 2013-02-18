using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IActualAgentHandler
	{
		RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId);
		IActualAgentState GetState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode,
		                           DateTime timestamp,
		                           TimeSpan timeInState, AutoResetEvent waitHandle);
	}

	public class ActualAgentHandler : IActualAgentHandler
	{
		private readonly IActualAgentStateDataHandler _actualAgentStateDataHandler;

		public ActualAgentHandler(IActualAgentStateDataHandler actualAgentStateDataHandler)
		{
			_actualAgentStateDataHandler = actualAgentStateDataHandler;
		}

		public IActualAgentState GetState(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode, DateTime timestamp,
			TimeSpan timeInState, AutoResetEvent waitHandle)
		{
			var scheduleLayers = _actualAgentStateDataHandler.CurrentLayerAndNext(timestamp, personId);
			var previousState = _actualAgentStateDataHandler.LoadOldState(personId);
			return CreateAndSaveState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timestamp, timeInState, businessUnitId, waitHandle);
		}

		public IActualAgentState CreateAndSaveState(IList<ScheduleLayer> scheduleLayers, IActualAgentState previousState, Guid personId, Guid platformTypeId,
			string stateCode, DateTime timeStamp, TimeSpan timeInState, Guid businessUnitId, AutoResetEvent waitHandle)
		{
			var scheduleLayer = scheduleLayers[0];
			var nextLayer = scheduleLayers[1];

			var foundAlarm = GetAlarm(platformTypeId, stateCode, scheduleLayer, businessUnitId);

			var newState = new ActualAgentState
			{
				PersonId = personId,
				StateCode = stateCode,
				AlarmStart = timeStamp,
				PlatformTypeId = platformTypeId,
				Timestamp = timeStamp
			};

			if (foundAlarm != null)
			{
				newState.AlarmName = foundAlarm.Name;
				newState.AlarmId = foundAlarm.AlarmTypeId;

				newState.Color = foundAlarm.DisplayColor;
				newState.StaffingEffect = foundAlarm.StaffingEffect;
				newState.State = foundAlarm.StateGroupName;
				newState.StateId = foundAlarm.StateGroupId;
				newState.StateStart = timeStamp.Add(timeInState.Negate());

				if (previousState != null && previousState.AlarmId == newState.AlarmId)
				{
					newState.StateStart = previousState.StateStart;
				}

			}
			if (scheduleLayer != null)
			{
				newState.Scheduled = scheduleLayer.Name;
				newState.ScheduledId = scheduleLayer.PayloadId;
				newState.StateStart = scheduleLayer.StartDateTime;
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

			// boxing??
			ThreadPool.QueueUserWorkItem(SaveToDataStore, new object[] { newState, waitHandle });
			//_actualAgentStateDataHandler.AddOrUpdate(newState);

			return newState;
		}

		private void SaveToDataStore(object arg)
		{
			var argsArr = arg as object[];
			if (argsArr == null) return;
			var agentState = argsArr[0] as IActualAgentState;
			var waitHandle = argsArr[1] as AutoResetEvent;

			if (agentState == null)
			{
				if (waitHandle != null) waitHandle.Set();
				return;
			}
			_actualAgentStateDataHandler.AddOrUpdate(agentState);
			if (waitHandle != null) waitHandle.Set();
		}

		public RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId)
		{
			var state = ResolveStateGroupId(platformTypeId, stateCode, businessUnitId);
		    var activityAlarms = _actualAgentStateDataHandler.ActivityAlarms();
		    var localPayloadId = PayloadId(layer);
		    return activityAlarms.ContainsKey(localPayloadId)
		               ? activityAlarms[localPayloadId].SingleOrDefault(
		                   s => s.StateGroupId == state)
		               : null;
		}

		private static Guid PayloadId(ScheduleLayer scheduleLayer)
		{
			return scheduleLayer == null ? Guid.Empty : scheduleLayer.PayloadId;
		}

		private Guid ResolveStateGroupId(Guid platformTypeId, string stateCode, Guid businessUnitId)
		{
			var stateGroups = _actualAgentStateDataHandler.StateGroups().ToList();
			var foundState = stateGroups.FirstOrDefault(s => s.PlatformTypeId == platformTypeId && s.StateCode == stateCode && s.BusinessUnitId == businessUnitId);
			return foundState != null ? foundState.StateGroupId : Guid.Empty;
		}
	}
}