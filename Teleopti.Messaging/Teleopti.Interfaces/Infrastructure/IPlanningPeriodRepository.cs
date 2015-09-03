using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPlanningPeriodRepository : IRepository<IPlanningPeriod>
	{
		IPlanningPeriodSuggestions Suggestions(INow now);
	}
}