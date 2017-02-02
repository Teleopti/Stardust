using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public interface INonBlendSkillCalculator
	{
		void Calculate(DateOnly day, IResourceCalculationDataContainer relevantProjections,
			IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> relevantResourceCalculationPeriods,
			bool addToEarlierResult);
	}
}