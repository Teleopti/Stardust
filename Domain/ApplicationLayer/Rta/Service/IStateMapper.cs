using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IStateMapper
	{
		StateMapping StateFor(Guid businessUnitId, Guid platformTypeId, string stateCode, string stateDescription);
		AlarmMapping AlarmFor(Guid businessUnitId, Guid platformTypeId, string stateCode, Guid? activityId);
	}
}