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

		public AlarmMappingInfo GetAlarm(Guid? activityId, Guid stateGroupId, Guid businessUnit)
		{
			return findAlarmForActivity(activityId ?? Guid.Empty, stateGroupId, businessUnit, _databaseReader.AlarmMappingInfos());
		}

		private static AlarmMappingInfo findAlarmForActivity(Guid activityId, Guid stateGroupId, Guid businessUnit,
			IDictionary<Tuple<Guid, Guid, Guid>, List<AlarmMappingInfo>> allAlarms)
		{
			List<AlarmMappingInfo> alarmForActivity;
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

		public StateCodeInfo StateCodeInfoFor(string stateCode, string stateDescription, Guid platformTypeId, Guid businessUnitId)
		{
			var stateCodeInfos = _databaseReader.StateCodeInfos();
			stateCode = stateCode ?? "";

			var match = (from s in stateCodeInfos
				where s.StateCode.ToUpper() == stateCode.ToUpper()
				      && s.PlatformTypeId == platformTypeId
				      && s.BusinessUnitId == businessUnitId
				select s).FirstOrDefault();

			if (match != null)
				return match;

			var newState = _databaseWriter.AddAndGetStateCode(stateCode, stateDescription, platformTypeId, businessUnitId);
			invalidateStateGroupCache();
			return newState;
		}

		private void invalidateStateGroupCache()
		{
			_mbCacheFactory.Invalidate(_databaseReader, x => x.StateCodeInfos(), false);
		}
	}
}