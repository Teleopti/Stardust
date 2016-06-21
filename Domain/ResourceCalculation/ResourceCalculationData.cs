using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationData : IResourceCalculationData
	{
		public ResourceCalculationData(ISchedulingResultStateHolder schedulingResultStateHolder, bool considerShortBreaks, bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = schedulingResultStateHolder.Schedules;
			Skills = schedulingResultStateHolder.Skills;
			SkillStaffPeriodHolder = schedulingResultStateHolder.SkillStaffPeriodHolder;
			SkillDays = schedulingResultStateHolder.SkillDays.SelectMany(s => s.Value); //TODO: check if perf drops due to this one
			SkipResourceCalculation = schedulingResultStateHolder.TeamLeaderMode || schedulingResultStateHolder.SkipResourceCalculation;
		}

		//public ResourceCalculationData(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> skills, IEnumerable<ISkillDay> skillDays, bool considerShortBreaks, bool doIntraIntervalCalculation)
		//{
		//	ConsiderShortBreaks = considerShortBreaks;
		//	DoIntraIntervalCalculation = doIntraIntervalCalculation;
		//	Schedules = scheduleDictionary;
		//	Skills = skills;
		//	SkillStaffPeriodHolder = new SkillStaffPeriodHolder();
		//	SkillDays = skillDays;
		//}

		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; }
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
		public IEnumerable<ISkillDay> SkillDays { get; }
		public bool SkipResourceCalculation { get; }
	}
}