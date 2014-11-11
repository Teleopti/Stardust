using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using log4net;
using MbCache.Core;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Web.Areas.Rta.Core.Server
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
			_mbCacheFactory.Invalidate(_databaseReader, x => x.StateGroups(), false);
		}
	}
}
