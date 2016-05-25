using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class FullResourceCalculationWithoutCascading : IFullResourceCalculation
	{
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public FullResourceCalculationWithoutCascading(DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void Execute()
		{
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary(new NoSchedulingProgress(), false);
		}
	}
}