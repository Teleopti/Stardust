using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios.MaxSeat
{
	public class MaxSeatOptimizationTeamTest : MaxSeatOptimizationTest
	{
		protected override OptimizationPreferences CreateOptimizationPreferences()
		{
			return new OptimizationPreferences
			{
				Extra =
				{
					UseTeams = true,
					TeamGroupPage = new GroupPageLight("_", GroupPageType.Hierarchy)
				}
			};
		}
	}
}