using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MbCache.Core;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class AlarmMapper
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;

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
			var allStateGroups = _databaseReader.StateGroups();
			List<RtaStateGroupLight> stateGroupsForStateCode;
			if (allStateGroups.TryGetValue(new Tuple<string, Guid, Guid>(stateCode.ToUpper(CultureInfo.InvariantCulture), platformTypeId, businessUnitId), out stateGroupsForStateCode))
			{
				return stateGroupsForStateCode.First();
			}
			
			var newState = _databaseWriter.AddAndGetNewRtaState(stateCode, platformTypeId,businessUnitId);
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
			_mbCacheFactory.Invalidate(_databaseReader, x => x.StateGroups(), false);
		}
	}
}
