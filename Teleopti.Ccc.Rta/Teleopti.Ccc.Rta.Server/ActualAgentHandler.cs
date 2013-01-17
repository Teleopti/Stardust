using System;
using System.Linq;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public class ActualAgentHandler : IActualAgentHandler
	{
		private readonly IActualAgentStateDataHandler _actualAgentStateDataHandler;
		//private static readonly ILog Logger = LogManager.GetLogger(typeof(ActualAgentHandler));

		public ActualAgentHandler(IActualAgentStateDataHandler actualAgentStateDataHandler)
		{
			_actualAgentStateDataHandler = actualAgentStateDataHandler;
		}

		public RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId)
		{
			var state = resolveStateGroupId(platformTypeId, stateCode, businessUnitId);
		    var activityAlarms = _actualAgentStateDataHandler.ActivityAlarms();
		    var localPayloadId = payloadId(layer);
		    return activityAlarms.ContainsKey(localPayloadId)
		               ? activityAlarms[localPayloadId].SingleOrDefault(
		                   s => s.StateGroupId == state)
		               : null;
		}

		private static Guid payloadId(ScheduleLayer scheduleLayer)
		{
			if (scheduleLayer == null) return Guid.Empty;
			return scheduleLayer.PayloadId;
		}

		private Guid resolveStateGroupId(Guid platformTypeId, string stateCode, Guid businessUnitId)
		{
			var stateGroups = _actualAgentStateDataHandler.StateGroups().ToArray();
			var foundState = stateGroups.FirstOrDefault(s => s.PlatformTypeId == platformTypeId && s.StateCode == stateCode && s.BusinessUnitId == businessUnitId);
			if (foundState != null)
			{
				return foundState.StateGroupId;
			}
			return Guid.Empty;
			//return addNewState(platformTypeId, stateCode, stateGroups);
		}

		//private static Guid addNewState(Guid platformTypeId, string stateCode, IEnumerable<RtaStateGroupLight> stateGroups)
		//{
		//    var defaultGroup = stateGroups.FirstOrDefault(s => s.DefaultStateGroup);
		//    if (defaultGroup != null)
		//    {
		//        return defaultGroup.AddState(stateCode,stateCode, platformTypeId);
		//    }

		//    Logger.WarnFormat(CultureInfo.CurrentCulture, "Could not find a default state group.");
		//    return null;
		//}
	}

	public interface IActualAgentHandler
	{
		RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer, Guid businessUnitId);
	}
}