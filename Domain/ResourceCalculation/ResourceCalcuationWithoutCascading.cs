using System;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalcuationWithoutCascading : IResourceCalculation
	{
		private readonly DoFullResourceOptimizationOneTime _doFullResourceOptimizationOneTime;

		public ResourceCalcuationWithoutCascading(DoFullResourceOptimizationOneTime doFullResourceOptimizationOneTime)
		{
			_doFullResourceOptimizationOneTime = doFullResourceOptimizationOneTime;
		}

		public void All()
		{
			_doFullResourceOptimizationOneTime.ExecuteIfNecessary(new NoSchedulingProgress(), false);
		}

		public void Period(DateOnlyPeriod period)
		{
			throw new NotImplementedException();
		}
	}
}