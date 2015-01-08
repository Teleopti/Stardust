using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta
{
	public interface IActualAgentAssembler
	{
		IActualAgentState GetAgentState(ExternalUserStateInputModel input, PersonOrganizationData person, ScheduleLayer currentLayer, ScheduleLayer nextLayer, IActualAgentState previousState, DateTime currentTime);
		IEnumerable<IActualAgentState> GetAgentStatesForMissingAgents(DateTime batchid, string sourceId);
	}
}