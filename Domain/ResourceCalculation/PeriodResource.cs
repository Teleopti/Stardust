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
		private readonly ConcurrentDictionary<ActivitySkillsCombination, InnerPeriodResourceDetail> _resourceDictionary = 
			new ConcurrentDictionary<ActivitySkillsCombination, InnerPeriodResourceDetail>();
		
		public void AppendResource(ActivitySkillsCombination key, SkillCombination skillCombination, double heads, double resource, DateTimePeriod? fractionPeriod)
		{
			_resourceDictionary.AddOrUpdate(key,
				new InnerPeriodResourceDetail(heads, resource, skillCombination.SkillEfficiencies,
					fractionPeriod.HasValue ? new[] {fractionPeriod.Value} : new DateTimePeriod[] {}),
				(s, d) =>
					new InnerPeriodResourceDetail(d.Count + heads, d.Resource + resource, mergeEffiencyResources(skillCombination.SkillEfficiencies,d.EffiencyResources),
						fractionPeriod.HasValue ? d.FractionPeriods.Append(fractionPeriod.Value).ToArray() : d.FractionPeriods.ToArray()));
		}

		private static SkillEffiencyResource[] mergeEffiencyResources(IEnumerable<SkillEffiencyResource> effiencyResources1,
			IEnumerable<SkillEffiencyResource> effiencyResources2)
		{
			var dic = new ConcurrentDictionary<Guid, efficiencyValueWrapper>();
			fillDictionaryWithSkillEffiency(dic, effiencyResources1);
			fillDictionaryWithSkillEffiency(dic, effiencyResources2);
			return dic.Select(keyValue => new SkillEffiencyResource(keyValue.Key, keyValue.Value.Current)).ToArray();
		}

		private static void fillDictionaryWithSkillEffiency(ConcurrentDictionary<Guid, efficiencyValueWrapper> dictionary, IEnumerable<SkillEffiencyResource> skillEffiencyResources)
		{
			foreach (var effiencyResource in skillEffiencyResources)
			{
				var item = dictionary.GetOrAdd(effiencyResource.Skill, _ => new efficiencyValueWrapper());
				item.Modify(effiencyResource.Resource);
			}
		}

		private class efficiencyValueWrapper
		{
			private double _innerValue;

			public void Modify(double change)
			{
				_innerValue = _innerValue + change;
			}

			public double Current => _innerValue;
		}

		private SkillEffiencyResource[] subtractEffiencyResources(SkillEffiencyResource[] baseCollection,
			SkillEffiencyResource[] subtractCollection)
		{
			var subtract = subtractCollection.Select(x => new SkillEffiencyResource(x.Skill, -x.Resource));
			var result = baseCollection.Concat(subtract);
			return result.GroupBy(x => x.Skill).Select(y => new SkillEffiencyResource(y.Key, Math.Max(y.Sum(z => z.Resource), 0))).ToArray();
		}

		public void RemoveResource(ActivitySkillsCombination key, SkillCombination skillCombination, double heads,  double resource, DateTimePeriod? fractionPeriod)
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
				var res = d.Resource - resource;
				if (res < 0 && Math.Round(res, 5) == 0.00000d)
				{
					res = 0;
				}
				return new InnerPeriodResourceDetail(d.Count - heads, res,
					subtractEffiencyResources(d.EffiencyResources, skillCombination.SkillEfficiencies), fractionPeriodResult);
			});
		}

		public PeriodResourceDetail GetResources(Guid skillKey, params Guid[] activityKeys)
		{
			var count = 0d;
			var resource = 0d;
			foreach (var pair in _resourceDictionary)
			{
				if (activityKeys.Any(a => pair.Key.HasActivity(a) || a == Guid.Empty) && pair.Key.ContainsSkill(skillKey))
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

		public IEnumerable<DateTimePeriod> GetFractionResources(Guid activityKey, Guid skillKey)
		{
			var result = _resourceDictionary.Where(
					pair => pair.Key.ContainsSkill(skillKey) && (Guid.Empty == activityKey || pair.Key.HasActivity(activityKey)))
				.SelectMany(pair => pair.Value.FractionPeriods)
				.ToArray();
			return result;
		}

		public IEnumerable<SkillKeyResource> GetSkillKeyResources(Guid activityKey)
		{
			return
				_resourceDictionary.Where(k => k.Key.HasActivity(activityKey))
					.Select(
						pair =>
							new SkillKeyResource
							{
								SkillKey = pair.Key.SkillCombinationKey(),
								Resource = new PeriodResourceDetail(pair.Value.Count, pair.Value.Resource),
								Effiencies = pair.Value.EffiencyResources
							});
		}
	}
}