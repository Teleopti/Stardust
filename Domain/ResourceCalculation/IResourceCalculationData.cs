using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public interface IResourceCalculationData
	{
		IScheduleDictionary Schedules { get; }
		bool ConsiderShortBreaks { get; }
		bool DoIntraIntervalCalculation { get; }
		IEnumerable<ISkill> Skills { get; }
		IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		bool SkipResourceCalculation { get; }
		SkillCombinationHolder SkillCombinationHolder {get;}
		ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }
		object ShovelingCallback { get; }
		void SetShovelingCallback(object shovelingCallback);
	}
}