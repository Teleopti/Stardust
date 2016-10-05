using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExtractSkillStaffDataForResourceCalculation : IExtractSkillStaffDataForResourceCalculation
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;
		private readonly IResourceCalculationContextFactory _resourceCalculationContextFactory;

		public ExtractSkillStaffDataForResourceCalculation(LoaderForResourceCalculation loaderForResourceCalculation, IResourceOptimizationHelper resourceOptimizationHelper, IResourceCalculationContextFactory resourceCalculationContextFactory)
		{
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
			_resourceCalculationContextFactory = resourceCalculationContextFactory;
		}

		public ISkillSkillStaffPeriodExtendedDictionary ExtractSkillStaffPeriodDictionary(DateOnlyPeriod periodDateOnly)
		{
			_loaderForResourceCalculation.PreFillInformation(periodDateOnly);

			var resCalcData = _loaderForResourceCalculation.ResourceCalculationData(periodDateOnly);
			DoCalculation(periodDateOnly, resCalcData);

			var skillStaffPeriodDictionary = resCalcData.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary;
			return skillStaffPeriodDictionary;
		}

		[LogTime]
		public virtual void DoCalculation(DateOnlyPeriod period, IResourceCalculationData resCalcData)
		{
			using (_resourceCalculationContextFactory.Create(resCalcData.Schedules, resCalcData.Skills))
			{
				ResourceCalculationContext.Fetch().PrimarySkillMode = true;
				_resourceOptimizationHelper.ResourceCalculate(period, resCalcData);
			}
		}

	}

	public interface IExtractSkillStaffDataForResourceCalculation
	{
		ISkillSkillStaffPeriodExtendedDictionary ExtractSkillStaffPeriodDictionary(DateOnlyPeriod periodDateOnly);
	}
}