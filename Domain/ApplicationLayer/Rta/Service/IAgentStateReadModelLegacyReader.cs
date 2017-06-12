using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.ApplicationLayer.Rta.ViewModels;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Rta.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> ReadFor(AgentStateFilter filter);
		IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	}
}