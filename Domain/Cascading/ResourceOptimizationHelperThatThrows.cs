using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.ResourceCalculation.IntraIntervalAnalyze;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Cascading
{
	public class ResourceOptimizationHelperThatThrows : ResourceOptimizationHelper
	{
		public ResourceOptimizationHelperThatThrows(IOccupiedSeatCalculator occupiedSeatCalculator, INonBlendSkillCalculator nonBlendSkillCalculator, IPersonSkillProvider personSkillProvider, IPeriodDistributionService periodDistributionService, IIntraIntervalFinderService intraIntervalFinderService, ITimeZoneGuard timeZoneGuard, CascadingResourceCalculationContextFactory resourceCalculationContextFactory) : base(occupiedSeatCalculator, nonBlendSkillCalculator, personSkillProvider, periodDistributionService, intraIntervalFinderService, timeZoneGuard, resourceCalculationContextFactory)
		{
		}

		public override void ResourceCalculate(DateOnly localDate, ResourceCalculationData resourceCalculationData)
		{
			throw new NotSupportedException("Res calc is not supported");
		}
	}
}