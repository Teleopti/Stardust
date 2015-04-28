using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Interfaces.Infrastructure
{
	public interface IPlanningPeriodSuggestions
	{
		DateOnlyPeriod Default(bool isEnabled);
		IEnumerable<SchedulePeriodType> UniqueSuggestedPeriod { get; }
	}
}