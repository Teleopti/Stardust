using System;
using System.Collections.Generic;
using System.Linq;
using MbCache.Core;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public interface IAlarmMapper
	{
		RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId);
		RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId);
		bool IsAgentLoggedOut(Guid scheduledId, string stateCode, Guid platformTypeId, Guid businessUnitId);
	}

	public class AlarmMapper : IAlarmMapper
	{
		private readonly IDatabaseHandler _databaseHandler;
		private readonly IMbCacheFactory _mbCacheFactory;
		private static readonly ILog Logger = LogManager.GetLogger(typeof (IAlarmMapper));

		public AlarmMapper(IDatabaseHandler databaseHandler, IMbCacheFactory mbCacheFactory)
		{
			_databaseHandler = databaseHandler;
			_mbCacheFactory = mbCacheFactory;
		}

		public RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId)
		{
			Logger.InfoFormat("Getting alarm for Activity: {0}, StateGroupId: {1}", activityId, stateGroupId);
			
			var allAlarms = _databaseHandler.ActivityAlarms();
			var alarm = findAlarmForActivity(activityId, stateGroupId, allAlarms);
			return alarm ?? findAlarmForActivity(Guid.Empty, stateGroupId, allAlarms);
		}
		
		private static RtaAlarmLight findAlarmForActivity(Guid activityId, Guid stateGroupId,
		                                                  IDictionary<Guid, List<RtaAlarmLight>> allAlarms)
		{
			Logger.InfoFormat("Trying to get alarm for Activity: {0}, StateGroupId: {1}", activityId, stateGroupId);
			List<RtaAlarmLight> alarmForActivity;
			if (allAlarms.TryGetValue(activityId, out alarmForActivity))
			{
				var alarmForStateGroup = alarmForActivity.SingleOrDefault(a => a.StateGroupId == stateGroupId);
				if (alarmForStateGroup != null)
					Logger.InfoFormat("Found Alarm: {0}, AlarmId: {1}", alarmForStateGroup.AlarmTypeId, alarmForStateGroup.Name);
				else
					Logger.InfoFormat("Could not find alarm");
				return alarmForStateGroup;
			}
			return null;
		}

		public RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var allStateGroups = _databaseHandler.StateGroups();
			List<RtaStateGroupLight> stateGroupsForStateCode;
			if (allStateGroups.TryGetValue(stateCode, out stateGroupsForStateCode))
			{
				var stateGroup = stateGroupsForStateCode.SingleOrDefault(s => s.BusinessUnitId == businessUnitId && s.PlatformTypeId == platformTypeId);
				if (stateGroup != null)
					return stateGroup;
			}
			else if (!string.IsNullOrEmpty(stateCode))
			{
				Logger.InfoFormat("Could not find SateCode: {0}, for PlatformTypeId: {1}, on BusinessUnit: {2}", stateCode, platformTypeId, businessUnitId);
				Logger.Info("Adding StateCode to database and clearing stategroup cache");
				var newState = _databaseHandler.AddAndGetNewRtaState(stateCode, platformTypeId);
				invalidateStateGroupCache();
				return newState;
			}
			return null;
		}

		public bool IsAgentLoggedOut(Guid scheduledId, string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			var state = GetStateGroup(stateCode, platformTypeId, businessUnitId);
			if (state != null)
			{
				var alarm = GetAlarm(scheduledId, state.StateGroupId);
				return alarm.IsLogOutState;
			}
			return false;
		}

		private void invalidateStateGroupCache()
		{
			_mbCacheFactory.Invalidate(_databaseHandler, x => x.StateGroups(), false);
		}
	}

	public class RtaStateGroupLight
	{
		public Guid StateId { get; set; }
		public string StateName { get; set; }
		public string StateGroupName { get; set; }
		public Guid PlatformTypeId { get; set; }
		public string StateCode { get; set; }
		public Guid StateGroupId { get; set; }
		public Guid BusinessUnitId { get; set; }
	}

	public class RtaAlarmLight
	{
		public Guid StateGroupId { get; set; }
		public string StateGroupName { get; set; }
		public Guid ActivityId { get; set; }
		public string Name { get; set; }
		public Guid AlarmTypeId { get; set; }
		public int DisplayColor { get; set; }
		public double StaffingEffect { get; set; }
		public long ThresholdTime { get; set; }
		public bool IsLogOutState { get; set; }
	}
}
