using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class SlimSkillResourceCalculationPeriodWrapper : ISkillResourceCalculationPeriodDictionary
	{
		private readonly IDictionary<ISkill, IResourceCalculationPeriodDictionary> _relevantSkillStaffPeriods;

		public SlimSkillResourceCalculationPeriodWrapper(IDictionary<ISkill, IResourceCalculationPeriodDictionary> relevantSkillStaffPeriods)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
		}

		public bool TryGetValue(ISkill skill, out IResourceCalculationPeriodDictionary resourceCalculationPeriods)
		{
			return _relevantSkillStaffPeriods.TryGetValue(skill, out resourceCalculationPeriods);
		}

		public bool IsOpen(ISkill skill, DateTimePeriod periodToCalculate)
		{
			IResourceCalculationPeriodDictionary resources;
			IResourceCalculationPeriod items;
			return _relevantSkillStaffPeriods.TryGetValue(skill, out resources) &&
					resources.TryGetValue(periodToCalculate, out items);
		}

		public IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> Items()
		{
			return _relevantSkillStaffPeriods;
		}

		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			if (_relevantSkillStaffPeriods.ContainsKey(skill))
			{
				var wrappedDictionary = _relevantSkillStaffPeriods[skill];

				IResourceCalculationPeriod skillStaffPeriod;
				if (wrappedDictionary.TryGetValue(period, out skillStaffPeriod))
				{
					dataForInterval = (IShovelResourceDataForInterval) skillStaffPeriod;
					return true;

				}
			}

			dataForInterval = null;
			return false;
		}
	}
}