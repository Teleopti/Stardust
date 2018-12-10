
namespace Teleopti.Ccc.Domain.InterfaceLegacy.Domain
{
	public interface IOccupiedSeatCalculator
	{
		void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections,
			ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods);
	}
}