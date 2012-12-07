using System;
using System.Linq;
using log4net;

namespace Teleopti.Ccc.Rta.Server
{
	public class ActualAgentHandler : IActualAgentHandler
	{
		private readonly IActualAgentStateDataHandler _actualAgentStateDataHandler;
		private static readonly ILog Logger = LogManager.GetLogger(typeof(ActualAgentHandler));

		public ActualAgentHandler(IActualAgentStateDataHandler actualAgentStateDataHandler)
		{
			_actualAgentStateDataHandler = actualAgentStateDataHandler;
		}

		public RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer)
		{
			var state = resolveStateGroupId(platformTypeId, stateCode);

			return _actualAgentStateDataHandler.ActivityAlarms().FirstOrDefault(
					a => a.ActivityId == payloadId(layer) && a.StateGroupId == state);
		}

		private static Guid payloadId(ScheduleLayer scheduleLayer)
		{
			if (scheduleLayer == null) return Guid.Empty;
			return scheduleLayer.PayloadId;
		}

		private Guid resolveStateGroupId(Guid platformTypeId, string stateCode)
		{
			var stateGroups = _actualAgentStateDataHandler.StateGroups().ToArray();
			var foundState = stateGroups.FirstOrDefault(s => s.PlatformTypeId == platformTypeId && s.StateCode == stateCode);
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
		RtaAlarmLight GetAlarm(Guid platformTypeId, string stateCode, ScheduleLayer layer);
	}
}