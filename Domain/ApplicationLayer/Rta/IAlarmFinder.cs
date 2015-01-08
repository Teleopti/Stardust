using System;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAlarmFinder
	{
		RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit);
		RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId);
	}
}