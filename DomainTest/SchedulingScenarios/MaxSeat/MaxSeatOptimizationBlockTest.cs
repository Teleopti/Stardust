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
				//TODO: Claes! Vad ska det defaultas till här?
				Extra =
				{
					UseTeamBlockOption = true,
					BlockTypeValue = BlockFinderType.SingleDay
				}
			};
		}
	}
}