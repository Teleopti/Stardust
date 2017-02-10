using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPlanningPeriodRepository : IRepository<IPlanningPeriod>
	{
		IPlanningPeriodSuggestions Suggestions(INow now);
		IEnumerable<IPlanningPeriod> LoadForAgentGroup(IAgentGroup agentGroup);
	}
}