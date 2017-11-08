using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExtractSkillStaffingDataForResourceCalculation
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IResourceCalculation _resourceOptimizationHelper;

		public ExtractSkillStaffingDataForResourceCalculation(LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceOptimizationHelper, CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public ResourceCalculationData ExtractResourceCalculationData(DateOnlyPeriod periodDateOnly)
		{
			_loaderForResourceCalculation.PreFillInformation(periodDateOnly);

			var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(periodDateOnly);
			DoCalculation(periodDateOnly, resCalcData);

			return resCalcData;
		}

		[TestLog]
		public virtual void DoCalculation(DateOnlyPeriod period, ResourceCalculationData resCalcData)
		{
			using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills, false, period))
			{
				_resourceOptimizationHelper.ResourceCalculate(period, resCalcData);
			}
		}
	}
}