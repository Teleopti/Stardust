using System.Collections.Generic;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationData
	{
		public ResourceCalculationData(ISchedulingResultStateHolder schedulingResultStateHolder, 
																bool considerShortBreaks, 
																bool doIntraIntervalCalculation)
			: this(schedulingResultStateHolder.Schedules, schedulingResultStateHolder.SkillDays, schedulingResultStateHolder.SkillStaffPeriodHolder, considerShortBreaks, doIntraIntervalCalculation)
		{
			SkipResourceCalculation = schedulingResultStateHolder.TeamLeaderMode || schedulingResultStateHolder.SkipResourceCalculation;
		}

		public ResourceCalculationData(IScheduleDictionary scheduleDictionary,
			IDictionary<ISkill, IEnumerable<ISkillDay>> skillDays,
			ISkillStaffPeriodHolder skillStaffPeriodHolder,
			bool considerShortBreaks,
			bool doIntraIntervalCalculation)
		{
			ConsiderShortBreaks = considerShortBreaks;
			DoIntraIntervalCalculation = doIntraIntervalCalculation;
			Schedules = scheduleDictionary;
			Skills = new HashSet<ISkill>(skillDays.Keys);
			SkillCombinationHolder = new SkillCombinationHolder();
			SkillDays = skillDays;
			SkillResourceCalculationPeriodDictionary =
				new SkillResourceCalculationPeriodWrapper((skillStaffPeriodHolder ?? new SkillStaffPeriodHolder(skillDays)).SkillSkillStaffPeriodDictionary);
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
		public IShovelingCallback ShovelingCallback { get; private set; } = new NoShovelingCallback();
		public void SetShovelingCallback(IShovelingCallback shovelingCallback)
		{
			ShovelingCallback = shovelingCallback;
		}
	}
}