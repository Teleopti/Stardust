using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExtractSkillStaffDataForResourceCalculation : IExtractSkillStaffDataForResourceCalculation
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceOptimization _resourceOptimizationHelper;
		private readonly CascadingResourceCalculationContextFactory _resourceCalculationContextFactory;

		public ExtractSkillStaffDataForResourceCalculation(LoaderForResourceCalculation loaderForResourceCalculation, IResourceOptimization resourceOptimizationHelper, CascadingResourceCalculationContextFactory resourceCalculationContextFactory)
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

		[TestLogTime]
		public virtual void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData)
		{
			using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills, true,period))
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