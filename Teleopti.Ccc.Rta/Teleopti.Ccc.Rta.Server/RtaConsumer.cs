using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IRtaConsumer
	{
		IActualAgentState Consume(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode, DateTime timeStamp, TimeSpan timeInState);
	}

	public class RtaConsumer : IRtaConsumer
	{
		private readonly IActualAgentStateDataHandler _actualAgentStateDataHandler;
		private readonly IActualAgentHandler _actualAgentHandler;

		public RtaConsumer(IActualAgentStateDataHandler actualAgentStateDataHandler, IActualAgentHandler actualAgentHandler)
		{
			_actualAgentStateDataHandler = actualAgentStateDataHandler;
			_actualAgentHandler = actualAgentHandler;
		}

		public IActualAgentState Consume(Guid personId, Guid businessUnitId, Guid platformTypeId, string stateCode, DateTime timeStamp,
			TimeSpan timeInState)
		{
			var scheduleLayers = _actualAgentStateDataHandler.CurrentLayerAndNext(timeStamp,personId);
			var previousState = _actualAgentStateDataHandler.LoadOldState(personId);
			return checkState(scheduleLayers, previousState, personId, platformTypeId, stateCode, timeStamp, timeInState, businessUnitId);
		}

		IActualAgentState checkState(IList<ScheduleLayer> scheduleLayers, IActualAgentState previousState, Guid personId, Guid platformTypeId,
			string stateCode, DateTime timeStamp, TimeSpan timeInState, Guid businessUnitId)
		{
			var scheduleLayer = scheduleLayers[0];
			var nextLayer = scheduleLayers[1];

			var foundAlarm = _actualAgentHandler.GetAlarm(platformTypeId, stateCode, scheduleLayer, businessUnitId);

			var newState = new ActualAgentState
			{
				PersonId = personId,
				StateCode = stateCode,
				AlarmStart = timeStamp,
				PlatformTypeId = platformTypeId
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

			_actualAgentStateDataHandler.AddOrUpdate(newState);

			return newState;
		}
	}
}