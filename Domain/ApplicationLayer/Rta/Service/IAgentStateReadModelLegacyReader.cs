using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> ReadFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds);
		IEnumerable<AgentStateReadModel> ReadInAlarmExcludingStatesFor(IEnumerable<Guid> siteIds, IEnumerable<Guid> teamIds, IEnumerable<Guid> skillIds, IEnumerable<Guid?> excludedStates);
	}

	public interface IAgentStateReadModelLegacyReader
    {
        IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons);
        IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
		IEnumerable<AgentStateReadModel> ReadForSites(IEnumerable<Guid> siteIds);
		IEnumerable<AgentStateReadModel> ReadForTeams(IEnumerable<Guid> teamIds);
	}
}