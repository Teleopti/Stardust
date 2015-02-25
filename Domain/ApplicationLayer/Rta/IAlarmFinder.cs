using System;
using Teleopti.Ccc.Domain.Rta;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IAlarmFinder
	{
		AlarmMappingInfo GetAlarm(Guid? activityId, Guid stateGroupId, Guid businessUnit);
		StateCodeInfo StateCodeInfoFor(string stateCode, string stateDescription, Guid platformTypeId, Guid businessUnitId);
	}
}