using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
	public interface IDayOffRulesRepository : IRepository<DayOffRules>
	{
		IList<DayOffRules> LoadAllByAgentGroup(IAgentGroup agentGroup);
		IList<DayOffRules> LoadAllWithoutAgentGroup();
		void RemoveForAgentGroup(IAgentGroup agentGroup);
	}
}