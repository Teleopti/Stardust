using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IFetchAgentGroupModel
	{
		IEnumerable<AgentGroupModel> FetchAll();
		AgentGroupModel Fetch(Guid id);
	}
}