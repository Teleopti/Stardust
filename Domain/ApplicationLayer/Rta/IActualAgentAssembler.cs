using System;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActualAgentAssembler
	{
		IActualAgentState GetAgentState(
			ScheduleLayer currentLayer,
			ScheduleLayer nextLayer,
			IActualAgentState previousState,
			Guid personId,
			Guid businessUnitId,
			Guid? platformTypeId,
			string stateCode,
			DateTime currentTime,
			DateTime? batchId,
			string originalSourceId);

		RtaStateGroupLight GetStateGroup(string stateCode, Guid platformTypeId, Guid businessUnitId);
		RtaAlarmLight GetAlarm(Guid activityId, Guid stateGroupId, Guid businessUnit);
	}
}