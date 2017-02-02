using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public class FakeResourceCalculationData : IResourceCalculationData
	{
		public void SetSkills(IList<ISkill> skills )
		{
			Skills = skills;
		}

		public void SetSkillStaffPeriodHolder(ISkillStaffPeriodHolder skillStaffPeriodHolder)
		{
			SkillStaffPeriodHolder = skillStaffPeriodHolder;
		}
		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; private set; }
		public ISkillStaffPeriodHolder SkillStaffPeriodHolder { get; private set; }
		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		public bool SkipResourceCalculation { get; }
		public SkillCombinationHolder SkillCombinationHolder { get; }
		public ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; }
	}
}