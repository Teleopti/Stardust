using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IPeriodDistributionService
	{
		void CalculateDay(IResourceCalculationDataContainer resourceContainer, ISkillResourceCalculationPeriodDictionary skillStaffPeriods);
	}
}