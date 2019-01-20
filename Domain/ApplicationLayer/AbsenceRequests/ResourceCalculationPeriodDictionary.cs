using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourceCalculationPeriodDictionary : IResourceCalculationPeriodDictionary
	{
		private readonly IDictionary<DateTimePeriod, IResourceCalculationPeriod> _relevantSkillStaffPeriods;

		public ResourceCalculationPeriodDictionary(IDictionary<DateTimePeriod, IResourceCalculationPeriod> relevantSkillStaffPeriods)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
		}

		public IEnumerable<KeyValuePair<DateTimePeriod, IResourceCalculationPeriod>> Items()
		{
			return _relevantSkillStaffPeriods;
		}

		public bool TryGetValue(DateTimePeriod dateTimePeriod, out IResourceCalculationPeriod resourceCalculationPeriod)
		{
			if (_relevantSkillStaffPeriods.ContainsKey(dateTimePeriod))
			{
				resourceCalculationPeriod = _relevantSkillStaffPeriods[dateTimePeriod];
				return true;
			}
			resourceCalculationPeriod = null;
			return false;
		}

		public IEnumerable<IResourceCalculationPeriod> OnlyValues()
		{
			return _relevantSkillStaffPeriods.Values;
		}
	}
}