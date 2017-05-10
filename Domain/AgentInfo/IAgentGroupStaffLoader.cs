using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public interface IAgentGroupStaffLoader
	{
		PeopleSelection Load(DateOnlyPeriod period, IAgentGroup agentGroup);
		int NumberOfAgents(DateOnlyPeriod period, IAgentGroup agentGroup);
		IList<Guid> LoadPersonIds(DateOnlyPeriod period, IAgentGroup agentGroup);
	}
}