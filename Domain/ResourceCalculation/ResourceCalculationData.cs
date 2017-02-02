using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationData : IResourceCalculationData
	{
		public ResourceCalculationData(ISchedulingResultStateHolder schedulingResultStateHolder, 
																bool considerShortBreaks, 
																bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = schedulingResultStateHolder.Schedules;
			Skills = schedulingResultStateHolder.Skills;
			SkillStaffPeriodHolder = schedulingResultStateHolder.SkillStaffPeriodHolder;
			SkillDays = schedulingResultStateHolder.SkillDays;
			SkipResourceCalculation = schedulingResultStateHolder.TeamLeaderMode || schedulingResultStateHolder.SkipResourceCalculation;
			SkillResourceCalculationPeriodDictionary =
				new SkillResourceCalculationPeriodWrapper(SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary);
		}

		public ResourceCalculationData(IScheduleDictionary scheduleDictionary, 
																	IEnumerable<ISkill> skills, 
																	IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays, 
																	bool considerShortBreaks, 
																	bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = scheduleDictionary;
			Skills = skills;
			SkillStaffPeriodHolder = new SkillStaffPeriodHolder(skillDays);
			SkillCombinationHolder = new SkillCombinationHolder();
			SkillDays = skillDays;
			SkillResourceCalculationPeriodDictionary =
				new SkillResourceCalculationPeriodWrapper(SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary);
		}
		public ResourceCalculationData(IEnumerable<ISkill> skills, ISkillResourceCalculationPeriodDictionary skillResourceCalculationPeriodDictionary)
		{
			ConsiderShortBreaks = false;
			DoIntraIntervalCalculation = false;
			Schedules = null;
			Skills = skills;
			SkillResourceCalculationPeriodDictionary = skillResourceCalculationPeriodDictionary;
		}

		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; }
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; }
		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		public bool SkipResourceCalculation { get; }
		public SkillCombinationHolder SkillCombinationHolder { get; }
		public ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }
		
	}
}