using System.Collections.Generic;
using Teleopti.Ccc.Domain.Cascading;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.DomainTest.ResourceCalculation
{
	public class FakeResourceCalculationData : IResourceCalculationData
	{
		public void SetSkills(IList<ISkill> skills )
		{
			Skills = skills;
		}

		public void SetISkillResourceCalculationPeriodDictionary(ISkillResourceCalculationPeriodDictionary skillResourceCalculationPeriodDictionary)
		{
			SkillResourceCalculationPeriodDictionary = skillResourceCalculationPeriodDictionary;
		}
		public IScheduleDictionary Schedules { get; }
		public bool ConsiderShortBreaks { get; }
		public bool DoIntraIntervalCalculation { get; }
		public IEnumerable<ISkill> Skills { get; private set; }
		public IDictionary<ISkill, IEnumerable<ISkillDay>> SkillDays { get; }
		public bool SkipResourceCalculation { get; }
		public SkillCombinationHolder SkillCombinationHolder { get; }
		public ISkillResourceCalculationPeriodDictionary SkillResourceCalculationPeriodDictionary { get; private set; }

		public IShovelingCallback ShovelingCallback
		{
			get { throw new System.NotImplementedException(); }
			set { throw new System.NotImplementedException(); }
		}

		public void SetShovelingCallback(IShovelingCallback shovelingCallback)
		{
			throw new System.NotImplementedException();
		}
	}
}