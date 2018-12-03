
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface INonBlendSkillCalculator
	{
		void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections,
			ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods, bool addToEarlierResult);
	}
}