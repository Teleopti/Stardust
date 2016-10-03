using Teleopti.Ccc.Domain.Common.TimeLogger;
using Teleopti.Ccc.Domain.Scheduling.WebLegacy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ExtractSkillStaffDataForResourceCalcualtion : IExtractSkillStaffDataForResourceCalcualtion
	{
		private readonly LoaderForResourceCalculation _loaderForResourceCalculation;
		private readonly IResourceOptimizationHelper _resourceOptimizationHelper;

		public ExtractSkillStaffDataForResourceCalcualtion(LoaderForResourceCalculation loaderForResourceCalculation, IResourceOptimizationHelper resourceOptimizationHelper)
		{
			_loaderForResourceCalculation = loaderForResourceCalculation;
			_resourceOptimizationHelper = resourceOptimizationHelper;
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
			foreach (var dateOnly in period.DayCollection())
			{
				_resourceOptimizationHelper.ResourceCalculate(dateOnly, resCalcData);
			}
		}
		
	}

	public interface IExtractSkillStaffDataForResourceCalcualtion
	{
		ISkillSkillStaffPeriodExtendedDictionary ExtractSkillStaffPeriodDictionary(DateOnlyPeriod periodDateOnly);
	}
}