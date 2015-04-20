using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PeriodResource
	{
		private readonly ConcurrentDictionary<string, InnerPeriodResourceDetail> _resourceDictionary = new ConcurrentDictionary<string, InnerPeriodResourceDetail>();
		
		public void AppendResource(string key, SkillCombination skillCombination, double heads, double resource, DateTimePeriod? fractionPeriod)
		{
			_resourceDictionary.AddOrUpdate(key,
				new InnerPeriodResourceDetail(heads, resource, skillCombination.SkillEfficiencies,
					fractionPeriod.HasValue ? new[] {fractionPeriod.Value} : new DateTimePeriod[] {}),
				(s, d) =>
					new InnerPeriodResourceDetail(d.Count + heads, d.Resource + resource, mergeEffiencyResources(skillCombination.SkillEfficiencies,d.EffiencyResources),
						fractionPeriod.HasValue ? d.FractionPeriods.Append(fractionPeriod.Value).ToArray() : d.FractionPeriods));
		}

		private SkillEffiencyResource[] mergeEffiencyResources(SkillEffiencyResource[] effiencyResources1,
			SkillEffiencyResource[] effiencyResources2)
		{
			return effiencyResources1.Join(effiencyResources2, e1 => e1.Skill, e2 => e2.Skill,
				(x, y) => new SkillEffiencyResource(x.Skill, x.Resource + y.Resource)).ToArray();
		}

		private SkillEffiencyResource[] subtractEffiencyResources(SkillEffiencyResource[] effiencyResources1,
			SkillEffiencyResource[] effiencyResources2)
		{
			return effiencyResources1.Join(effiencyResources2, e1 => e1.Skill, e2 => e2.Skill,
				(x, y) => new SkillEffiencyResource(x.Skill, x.Resource - y.Resource)).ToArray();
		}

		public void RemoveResource(string key, SkillCombination skillCombination, double resource, DateTimePeriod? fractionPeriod)
		{
			_resourceDictionary.AddOrUpdate(key, new InnerPeriodResourceDetail(0, 0, skillCombination.SkillEfficiencies, new DateTimePeriod[]{}), (s, d) =>
			{
				var fractionPeriodResult = d.FractionPeriods;
				if (fractionPeriod.HasValue)
				{
					List<DateTimePeriod> dateTimePeriods = d.FractionPeriods.ToList();
					dateTimePeriods.Remove(fractionPeriod.Value);
					fractionPeriodResult = dateTimePeriods.ToArray();
				}
				return new InnerPeriodResourceDetail(d.Count - 1, d.Resource - resource,
					subtractEffiencyResources(d.EffiencyResources, skillCombination.SkillEfficiencies), fractionPeriodResult);
			});
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
					var foundEfficiency = pair.Value.EffiencyResources.FirstOrDefault(s => s.Skill == skillKey);
					if (foundEfficiency.Skill == skillKey)
					{
						currentResource = currentResource * foundEfficiency.Resource;
					}
					
					count += pair.Value.Resource;
					resource += currentResource;
				}
			}
			return new PeriodResourceDetail(count, resource);
		}

		public IEnumerable<DateTimePeriod> GetFractionResources(string activityKey, Guid skillKey)
		{
			var result = new List<DateTimePeriod>();
			var skillKeyString = skillKey.ToString();
			foreach (var pair in _resourceDictionary)
			{
				if ((string.IsNullOrEmpty(activityKey) || pair.Key.StartsWith(activityKey)) && (pair.Key.Contains(skillKeyString)))
				{
					result.AddRange(pair.Value.FractionPeriods);
				}
			}
			return result;
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
					var foundEfficiency = pair.Value.EffiencyResources.FirstOrDefault(s => s.Skill == skillKey);
					if (foundEfficiency.Skill == skillKey)
					{
						currentResource = currentResource * foundEfficiency.Resource;
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

				var skillKey = pair.Key.Split('|')[1];
				yield return new SkillKeyResource { SkillKey = skillKey, Resource = new PeriodResourceDetail(pair.Value.Count,pair.Value.Resource), Effiencies = pair.Value.EffiencyResources };
			}
		}
	}
}