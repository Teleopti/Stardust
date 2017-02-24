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
			SkillDays = schedulingResultStateHolder.SkillDays;
			SkipResourceCalculation = schedulingResultStateHolder.TeamLeaderMode || schedulingResultStateHolder.SkipResourceCalculation;
			SkillResourceCalculationPeriodDictionary =
				new SkillResourceCalculationPeriodWrapper(schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary);
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
			SkillCombinationHolder = new SkillCombinationHolder();
			SkillDays = skillDays;
			SkillResourceCalculationPeriodDictionary =
				new SkillResourceCalculationPeriodWrapper(new SkillStaffPeriodHolder(skillDays).SkillSkillStaffPeriodDictionary);
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
		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		public bool SkipResourceCalculation { get; }
		public SkillCombinationHolder SkillCombinationHolder { get; }
		public ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }

		//"object" here should be IShovelingCallback but this !""#¤%"#¤#"¤"#¤ interface assembly prevent that (if I don't want to create interfaces for everything)
		//change this when #34355 is done
		public object ShovelingCallback { get; private set; } = new NoShovelingCallback();
		public void SetShovelingCallback(object shovelingCallback)
		{
			ShovelingCallback = shovelingCallback;
		}
	}
}