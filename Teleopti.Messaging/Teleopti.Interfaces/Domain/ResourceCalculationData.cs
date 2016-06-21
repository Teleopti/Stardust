using System.Collections.Generic;

namespace Teleopti.Interfaces.Domain
{
	public class ResourceCalculationData
	{
		public ResourceCalculationData(ISchedulingResultStateHolder schedulingResultStateHolder, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = schedulingResultStateHolder.Schedules;
			Skills = schedulingResultStateHolder.Skills;
			SkillStaffPeriodHolder = schedulingResultStateHolder.SkillStaffPeriodHolder;
		}

		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; }
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
	}
}