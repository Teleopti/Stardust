using System;
using System.Collections.Generic;
using System.Linq;
using MbCache.Core;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
    public class AlarmMapper : IAlarmMapper
	{
		private readonly IDatabaseReader _databaseReader;
        private readonly IDatabaseWriter _databaseWriter;
        private readonly IMbCacheFactory _mbCacheFactory;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(IAlarmMapper));

		public AlarmMapper(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory)
		{
			_databaseReader = databaseReader;
		    _databaseWriter = databaseWriter;
		    _mbCacheFactory = mbCacheFactory;
		}

		public RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit)
		{
			var alarm = findAlarmForActivity(activityId, stateGroupId, businessUnit, _databaseReader.ActivityAlarms());
			return alarm;
		}

        private static RtaAlarmLight findAlarmForActivity(Guid activityId, Guid stateGroupId, Guid businessUnit,
                                                          IDictionary<Tuple<Guid, Guid, Guid>, List<RtaAlarmLight>> allAlarms)
		{
			Logger.DebugFormat("Trying to get alarm for Activity: {0}, StateGroupId: {1}", activityId, stateGroupId);
			List<RtaAlarmLight> alarmForActivity;
			if (allAlarms.TryGetValue(new Tuple<Guid, Guid, Guid>(activityId,stateGroupId,businessUnit), out alarmForActivity))
			{
				var alarmForStateGroup = alarmForActivity.FirstOrDefault();
			    if (Logger.IsDebugEnabled)
			    {
			        if (alarmForStateGroup != null)
			            Logger.DebugFormat("Found Alarm: {0}, AlarmId: {1}", alarmForStateGroup.AlarmTypeId, alarmForStateGroup.Name);
			        else
			            Logger.DebugFormat("Could not find alarm, no matching StateGroupId");
			    }
			    return alarmForStateGroup;
			}
			return activityId != Guid.Empty
					   ? findAlarmForActivity(Guid.Empty, stateGroupId, businessUnit, allAlarms)
					   : null;
		}

		public RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			Logger.DebugFormat("Trying to get stategroup for StateCode: {0}, PlatformTypeId: {1}, BusinessUntiId: {2}", stateCode, platformTypeId, businessUnitId);
			var allStateGroups = _databaseReader.StateGroups();
			List<RtaStateGroupLight> stateGroupsForStateCode;
            if (allStateGroups.TryGetValue(new Tuple<string, Guid, Guid>(stateCode, platformTypeId, businessUnitId), out stateGroupsForStateCode))
			{
				var stateGroup = stateGroupsForStateCode.FirstOrDefault();
				if (stateGroup != null)
				{
                    if (Logger.IsDebugEnabled)
                    {
                        Logger.DebugFormat("Found StateGroupId: {0}, StateGroupName: {1}", stateGroup.StateGroupId, stateGroup.StateGroupName);
                    }
					return stateGroup;
				}
				Logger.InfoFormat(
					"Could not find stategroup connected to statecode {0}, for BusinessUnit: {1} and PlatformTypeId: {2}", stateCode,
					businessUnitId, platformTypeId);
			}
			else if (!string.IsNullOrEmpty(stateCode))
			{
                if (Logger.IsInfoEnabled)
				    Logger.InfoFormat("Could not find StateCode: {0}, for PlatformTypeId: {1}, on BusinessUnit: {2}", stateCode, platformTypeId, businessUnitId);

				Logger.Debug("Adding StateCode to database and clearing stategroup cache");
				var newState = _databaseWriter.AddAndGetNewRtaState(stateCode, platformTypeId,businessUnitId);
				invalidateStateGroupCache();
				return newState;
			}
			return null;
		}

		public bool IsAgentLoggedOut(Guid personId, string stateCode, Guid platformTypeId, Guid businessUnitId)
		{
			Logger.DebugFormat("Checking if agent is already in a stategroup marked as logged out state, personId: {0}", personId);
			var state = GetStateGroup(stateCode, platformTypeId, businessUnitId);
			if (state != null)
			{
				Logger.DebugFormat(state.IsLogOutState
									  ? "Agent: {0} is already logged out"
									  : "Agent: {0} is not logged out",
								  personId);
				return state.IsLogOutState;
			}
			return false;
		}

		private void invalidateStateGroupCache()
		{
			Logger.Debug("Clearing cache for state groups");
			_mbCacheFactory.Invalidate(_databaseReader, x => x.StateGroups(), false);
		}
	}
}
