using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IStateMapper
	{
		StateMapping StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription);
		AlarmMapping AlarmFor(Guid businessUnitId, string stateCode, Guid? activityId);
	}
}