using System;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.FeatureFlags;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.SchedulingScenarios
{
	// We also use this to test waitlist handling -> Don't remove when you remove toggle
	[RemoveMeWithToggle("ATM this is only used when toggle is true", Toggles.ResourcePlanner_MergeTeamblockClassicScheduling_44289)]
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