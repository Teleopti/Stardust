using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.RealTimeAdherence.ApplicationLayer.ViewModels;

namespace Teleopti.Ccc.Domain.RealTimeAdherence.Domain.Service
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter);
		IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	}
}