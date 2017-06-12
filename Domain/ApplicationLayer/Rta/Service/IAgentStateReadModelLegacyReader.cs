using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> ReadFor(AgentStateFilter filter);
	}

	public interface IAgentStateReadModelLegacyReader
    {
        IEnumerable<AgentStateReadModel> Read(IEnumerable<IPerson> persons);
        IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
    }
}