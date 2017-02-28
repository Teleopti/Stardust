using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.InterfaceLegacy.Infrastructure
{
	public interface IPlanningPeriodRepository : IRepository<IPlanningPeriod>
	{
		IPlanningPeriodSuggestions Suggestions(INow now);
		IEnumerable<IPlanningPeriod> LoadForAgentGroup(IAgentGroup agentGroup);
	}
}