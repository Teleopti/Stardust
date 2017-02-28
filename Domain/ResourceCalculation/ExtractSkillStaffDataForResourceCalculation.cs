using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExtractCascadingSkillStaffDataForResourceCalculation : IExtractSkillStaffDataForResourceCalculation
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;
		private readonly IResourceCalculation _resourceOptimizationHelper;

		public ExtractCascadingSkillStaffDataForResourceCalculation(LoaderForResourceCalculation loaderForResourceCalculation, IResourceCalculation resourceOptimizationHelper, CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public IResourceCalculationData ExtractResourceCalculationData(DateOnlyPeriod periodDateOnly)
		{
			_loaderForResourceCalculation.PreFillInformation(periodDateOnly);

			var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(periodDateOnly);
			DoCalculation(periodDateOnly, resCalcData);

			return resCalcData;
		}

		[TestLog]
		public virtual void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData)
		{
			using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills, false, period))
			{
				_resourceOptimizationHelper.ResourceCalculate(period, resCalcData);
			}
		}
	}

	public interface IExtractSkillStaffDataForResourceCalculation
	{
		IResourceCalculationData ExtractResourceCalculationData(DateOnlyPeriod periodDateOnly);
		void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData);
	}
}