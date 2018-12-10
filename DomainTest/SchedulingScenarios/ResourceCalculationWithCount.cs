using System;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;


namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	public class ResourceCalculationWithCount : CascadingResourceCalculation
	{
		public ResourceCalculationWithCount(ResourceOptimizationHelper resourceOptimizationHelper, ShovelResources shovelResources) : base(resourceOptimizationHelper, shovelResources)
		{
		}

		public int NumberOfCalculationsOnSingleDay { get; private set; }
		public int NumberOfCalls { get; private set; }

		public override void ResourceCalculate(DateOnlyPeriod period, ResourceCalculationData resourceCalculationData, Func<IDisposable> getResourceCalculationContext = null)
		{
			base.ResourceCalculate(period, resourceCalculationData, getResourceCalculationContext);
			NumberOfCalls++;
			if (period.DayCount() == 1)
			{
				NumberOfCalculationsOnSingleDay++;
			}
		}
	}
}