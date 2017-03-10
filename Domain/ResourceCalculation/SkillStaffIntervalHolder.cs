using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class SkillStaffIntervalHolder : IShovelResourceData
	{
		private readonly IDictionary<ISkill, IResourceCalculationPeriod> _dictionary;

		public SkillStaffIntervalHolder(IDictionary<ISkill, IResourceCalculationPeriod> dictionary)
		{
			_dictionary = dictionary;
			_dictionary = dictionary;
		}
		public bool TryGetDataForInterval(ISkill skill, DateTimePeriod period, out IShovelResourceDataForInterval dataForInterval)
		{
			IResourceCalculationPeriod resourceCalculationPeriod;
			if (_dictionary.TryGetValue(skill, out resourceCalculationPeriod))
			{
				var staffingInterval = (SkillStaffingInterval)resourceCalculationPeriod;
				if (period == new DateTimePeriod(staffingInterval.StartDateTime.Utc(), staffingInterval.EndDateTime.Utc()))
				{
					dataForInterval = staffingInterval;
					return true;
				}
			}
			dataForInterval = null;
			return false;
		}
	}
}