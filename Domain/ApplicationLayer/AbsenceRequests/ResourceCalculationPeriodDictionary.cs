using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

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
			return _relevantSkillStaffPeriods.TryGetValue(dateTimePeriod, out resourceCalculationPeriod);
		}

		public IEnumerable<IResourceCalculationPeriod> OnlyValues()
		{
			return _relevantSkillStaffPeriods.Values;
		}
	}
}