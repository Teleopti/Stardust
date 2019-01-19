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
			return _relevantSkillStaffPeriods.TryGetValue(skill, out var resources) &&
					resources.TryGetValue(periodToCalculate, out _);
		}

		public IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> Items()
		{
			return _relevantSkillStaffPeriods;
		}

		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			if (_relevantSkillStaffPeriods.TryGetValue(skill, out var wrappedDictionary))
			{
				if (wrappedDictionary.TryGetValue(period, out var skillStaffPeriod))
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