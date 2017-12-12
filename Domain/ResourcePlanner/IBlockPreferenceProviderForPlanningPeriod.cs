using System;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public interface IBlockPreferenceProviderForPlanningPeriod
	{
		IBlockPreferenceProvider Fetch(Guid planningPeriodId);
	}
}