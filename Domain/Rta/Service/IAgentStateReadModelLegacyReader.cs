using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Rta.ViewModels;

namespace Teleopti.Ccc.Domain.Rta.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter);
		IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	}
}