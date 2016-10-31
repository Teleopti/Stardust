using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	public class MaxSeatOptimizationBlockTest : MaxSeatOptimizationTest
	{
		protected override OptimizationPreferences CreateOptimizationPreferences()
		{
			return new OptimizationPreferences
			{
				Extra =
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SchedulePeriod
				}
			};
		}
	}
}