using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActualAgentAssembler
	{
		IActualAgentState GetAgentState(ExternalUserStateInputModel input, PersonWithBusinessUnit person, ScheduleLayer currentLayer, ScheduleLayer nextLayer, IActualAgentState previousState, DateTime currentTime);

		RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId);

		RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit);
	}
}