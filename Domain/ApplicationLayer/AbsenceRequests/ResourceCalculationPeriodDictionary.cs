using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class ResourceCalculationPeriodDictionary : IResourceCalculationPeriodDictionary
	{
		private readonly IDictionary<DateTimePeriod, IResourceCalculationPeriod> _relevantSkillStaffPeriods;
		private readonly IDictionary<DateTime, HashSet<DateTimePeriod>> _index;

		public ResourceCalculationPeriodDictionary(IDictionary<DateTimePeriod, IResourceCalculationPeriod> relevantSkillStaffPeriods)
		{
			_relevantSkillStaffPeriods = relevantSkillStaffPeriods;
			_index = relevantSkillStaffPeriods.Keys.SelectMany(k => k.Keys().Select(p => (p, k))).GroupBy(k => k.Item1)
				.ToDictionary(k => k.Key, v => v.Select(i => i.Item2).ToHashSet());
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

		public IEnumerable<IResourceCalculationPeriod> FindUsingIndex(DateTimePeriod period)
		{
			var keys = period.Keys();
			foreach (var key in keys)
			{
				if (!_index.TryGetValue(key, out var innerKeys)) continue;
				foreach (var innerKey in innerKeys)
				{
					if (innerKey.Intersect(period) && _relevantSkillStaffPeriods.TryGetValue(innerKey, out var skillStaffPeriod))
					{
						yield return skillStaffPeriod;
					}
				}
			}
		}
	}
}