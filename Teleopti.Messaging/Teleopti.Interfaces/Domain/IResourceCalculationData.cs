using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	//This interface is not needed but circle ref otherwise. Need to get rid of interface assembly first!
	public interface IResourceCalculationData
	{
		IScheduleDictionary Schedules { get; }
		bool ConsiderShortBreaks { get; }
		bool DoIntraIntervalCalculation { get; }
		IEnumerable<ISkill> Skills { get; }
		ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
		IEnumerable<ISkillDay> SkillDays { get; }
		bool SkipResourceCalculation { get; }
	}
}