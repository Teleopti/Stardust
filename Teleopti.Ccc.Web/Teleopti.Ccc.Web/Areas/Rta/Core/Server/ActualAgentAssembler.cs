using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class ActualAgentAssembler : IActualAgentAssembler
	{
		protected IDatabaseReader DatabaseReader;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;

		public ActualAgentAssembler(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory)
		{
			DatabaseReader = databaseReader;
			_databaseWriter = databaseWriter;
			_mbCacheFactory = mbCacheFactory;
		}

		public IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId)
		{
			var missingAgents = DatabaseReader.GetMissingAgentStatesFromBatch(batchid, sourceId);
			var agentsNotAlreadyLoggedOut = from a in missingAgents
				where !IsAgentLoggedOut(
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
			Guid? platformTypeId,
			string stateCode,
			DateTime currentTime,
			DateTime? batchId,
			string originalSourceId)
		{
			if (!batchId.HasValue)
				batchId = previousState.BatchId;
			if (!platformTypeId.HasValue)
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
				AlarmStart = currentTime,
				PlatformTypeId = platformTypeId.Value,
				ReceivedTime = currentTime,
				OriginalDataSourceId = originalSourceId,
				BusinessUnitId = businessUnitId
			};

			if (batchId.HasValue)
				newState.BatchId = batchId.Value;

			var state = GetStateGroup(stateCode, platformTypeId.Value, businessUnitId);
			var foundAlarm = GetAlarm(activityId, state.StateGroupId, businessUnitId);

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

		public RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit)
		{
			return findAlarmForActivity(activityId, stateGroupId, businessUnit, DatabaseReader.ActivityAlarms());
		}

		private static RtaAlarmLight findAlarmForActivity(Guid activityId, Guid stateGroupId, Guid businessUnit,
														  IDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> allAlarms)
		{
			List<RtaAlarmLight> alarmForActivity;
			if (allAlarms.TryGetValue(new Tuple<Guid, Guid, Guid>(activityId, stateGroupId, businessUnit), out alarmForActivity))
			{
				var alarmForStateGroup = alarmForActivity.FirstOrDefault();
				if (alarmForStateGroup != null && alarmForStateGroup.AlarmTypeId == Guid.Empty)
					return null;

				return alarmForStateGroup;
			}
			return activityId != Guid.Empty
					   ? findAlarmForActivity(Guid.Empty, stateGroupId, businessUnit, allAlarms)
					   : null;
		}

		public RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var allStateGroups = DatabaseReader.StateGroups();
			List<RtaStateGroupLight> stateGroupsForStateCode;
			if (stateCode == null) stateCode = "";
			var tuple = new Tuple<string, Guid, Guid>(stateCode.ToUpper(CultureInfo.InvariantCulture), platformTypeId, businessUnitId);
			if (allStateGroups.TryGetValue(tuple, out stateGroupsForStateCode))
			{
				return stateGroupsForStateCode.First();
			}

			var newState = _databaseWriter.AddAndGetNewRtaState(stateCode, platformTypeId, businessUnitId);
			invalidateStateGroupCache();
			return newState;
		}

		public bool IsAgentLoggedOut(Guid personId, string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var state = GetStateGroup(stateCode, platformTypeId, businessUnitId);
			return state != null && state.IsLogOutState;
		}

		private void invalidateStateGroupCache()
		{
			_mbCacheFactory.Invalidate(DatabaseReader, x => x.StateGroups(), false);
		}
	}
}