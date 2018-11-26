using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.ResourcePlanner
{
	public interface IBlockPreferenceProviderForPlanningPeriod
	{
		IBlockPreferenceProvider Fetch(AllSettingsForPlanningGroup allSettingsForPlanningGroup);
	}
}