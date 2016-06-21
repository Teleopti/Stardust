using System.Collections.Generic;
using System.Linq;

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
			SkillDays = schedulingResultStateHolder.SkillDays.SelectMany(s => s.Value); //TODO: check if perf drops due to this one
		}

		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; }
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
		public IEnumerable<ISkillDay> SkillDays { get; }
	}
}