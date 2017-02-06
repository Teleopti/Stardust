
namespace Teleopti.Interfaces.Domain
{
	public interface IOccupiedSeatCalculator
	{
		void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections,
			ISkillResourceCalculationPeriodDictionary relevantSkillStaffPeriods);
	}
}