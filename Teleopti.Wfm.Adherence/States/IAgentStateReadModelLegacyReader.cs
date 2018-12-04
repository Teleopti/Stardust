using System;
using System.Collections.Generic;
using Teleopti.Wfm.Adherence.ApplicationLayer.ViewModels;

namespace Teleopti.Wfm.Adherence.States
{
	public interface IAgentStateReadModelReader
	{
		IEnumerable<AgentStateReadModel> Read(AgentStateFilter filter);
		IEnumerable<AgentStateReadModel> Read(IEnumerable<Guid> personIds);
	}
}