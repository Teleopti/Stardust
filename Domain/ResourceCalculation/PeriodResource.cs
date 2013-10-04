using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PeriodResource
	{
		private readonly ConcurrentDictionary<string, PeriodResourceDetail> _resourceDictionary = new ConcurrentDictionary<string, PeriodResourceDetail>();
		private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, double>> _skillEffiencies = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, double>>();
		
		public void AppendResource(string key, SkillCombination skillCombination, double heads, double resource)
		{
			_resourceDictionary.AddOrUpdate(key, new PeriodResourceDetail(heads, resource), (s, d) => new PeriodResourceDetail(d.Count + heads, d.Resource + resource));

			foreach (var skillEfficiency in skillCombination.SkillEfficiencies)
			{
				_skillEffiencies.AddOrUpdate(key,
				                             new ConcurrentDictionary<Guid, double>(new[] { new KeyValuePair<Guid, double>(skillEfficiency.Key, skillEfficiency.Value) }),
				                             (s, doubles) =>
					                             {
						                             doubles.AddOrUpdate(skillEfficiency.Key, skillEfficiency.Value,
						                                                 (guid, d) => d + skillEfficiency.Value);
						                             return doubles;
					                             });
			}
		}

		public void RemoveResource(string key, SkillCombination skillCombination, double resource)
		{
			_resourceDictionary.AddOrUpdate(key, new PeriodResourceDetail(0, 0), (s, d) => new PeriodResourceDetail(d.Count - 1, d.Resource - resource));

			foreach (var skillEfficiency in skillCombination.SkillEfficiencies)
			{
				_skillEffiencies.AddOrUpdate(key,
				                             new ConcurrentDictionary<Guid, double>(),
				                             (s, doubles) =>
					                             {
						                             doubles.AddOrUpdate(skillEfficiency.Key, skillEfficiency.Value,
						                                                 (guid, d) => d - skillEfficiency.Value);
						                             return doubles;
					                             });
			}
		}

		public PeriodResourceDetail GetResources(string activityKey, Guid skillKey)
		{
			var count = 0d;
			var resource = 0d;
			var skillKeyString = skillKey.ToString();
			foreach (var pair in _resourceDictionary)
			{
				if ((string.IsNullOrEmpty(activityKey) || pair.Key.StartsWith(activityKey)) && (pair.Key.Contains(skillKeyString)))
				{
					double currentResource = pair.Value.Resource;
					ConcurrentDictionary<Guid, double> effiencies;
					if (_skillEffiencies.TryGetValue(pair.Key, out effiencies))
					{
						double effiency;
						if (effiencies.TryGetValue(skillKey, out effiency))
							currentResource = currentResource * effiency;
					}

					count += pair.Value.Resource;
					resource += currentResource;
				}
			}
			return new PeriodResourceDetail(count, resource);
		}

		public PeriodResourceDetail GetResources(IEnumerable<string> activityKeys, Guid skillKey)
		{
			var count = 0d;
			var resource = 0d;
			var skillKeyString = skillKey.ToString();
			foreach (var pair in _resourceDictionary)
			{
				if (pair.Key.Contains(skillKeyString) && activityKeys.Any(a => pair.Key.StartsWith(a)))
				{
					double currentResource = pair.Value.Resource;
					ConcurrentDictionary<Guid, double> effiencies;
					if (_skillEffiencies.TryGetValue(pair.Key, out effiencies))
					{
						double effiency;
						if (effiencies.TryGetValue(skillKey, out effiency))
							currentResource = currentResource * effiency;
					}

					count += pair.Value.Count;
					resource += currentResource;
				}
			}
			return new PeriodResourceDetail(count, resource);
		}

		public IEnumerable<SkillKeyResource> GetSkillKeyResources(string activityKey)
		{
			foreach (var pair in _resourceDictionary)
			{
				if (!pair.Key.StartsWith(activityKey)) continue;

				ConcurrentDictionary<Guid, double> effiencies;
				if (!_skillEffiencies.TryGetValue(pair.Key, out effiencies))
				{
					effiencies = new ConcurrentDictionary<Guid, double>();
				}

				var skillKey = pair.Key.Split('|')[1];
				yield return new SkillKeyResource { SkillKey = skillKey, Resource = pair.Value, Effiencies = effiencies };
			}
		}
	}
}