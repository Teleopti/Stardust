using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using MbCache.Core;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
{
	public class AlarmFinder : IAlarmFinder
	{
		private readonly IDatabaseReader _databaseReader;
		private readonly IDatabaseWriter _databaseWriter;
		private readonly IMbCacheFactory _mbCacheFactory;

		public AlarmFinder(IDatabaseReader databaseReader, IDatabaseWriter databaseWriter, IMbCacheFactory mbCacheFactory)
		{
			_databaseReader = databaseReader;
			_databaseWriter = databaseWriter;
			_mbCacheFactory = mbCacheFactory;
		}

		public RtaAlarmLight GetAlarm(Guid? activityId, Guid stateGroupId, Guid businessUnit)
		{
			return findAlarmForActivity(activityId ?? Guid.Empty, stateGroupId, businessUnit, _databaseReader.ActivityAlarms());
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

		private void invalidateStateGroupCache()
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.StateGroups(), false);
		}
	}
}