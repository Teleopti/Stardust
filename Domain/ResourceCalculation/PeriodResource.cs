using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class PeriodResource
	{
		private readonly ConcurrentDictionary<ActivitySkillsCombination, InnerPeriodResourceDetail> _resourceDictionary = 
			new ConcurrentDictionary<ActivitySkillsCombination, InnerPeriodResourceDetail>();
		
		public void AppendResource(ActivitySkillsCombination key, SkillCombination skillCombination, double heads, double resource, DateTimePeriod? fractionPeriod)
		{
			var fractionPeriods = fractionPeriod.HasValue ? new[] {fractionPeriod.Value} : new DateTimePeriod[] {};
			_resourceDictionary.AddOrUpdate(key, new InnerPeriodResourceDetail(heads, resource, skillCombination.SkillEfficiencies, fractionPeriods),
				(combination, detail) =>
					new InnerPeriodResourceDetail(
						detail.Count + heads, detail.Resource + resource,
						mergeEffiencyResources(skillCombination.SkillEfficiencies, detail.EffiencyResources),
						fractionPeriod.HasValue
							? detail.FractionPeriods.Append(fractionPeriod.Value).ToArray()
							: detail.FractionPeriods.ToArray()
					));
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
			SkillEffiencyResource[] subtractCollection, double weight)
		{
			var subtract = subtractCollection.Select(x => new SkillEffiencyResource(x.Skill, -x.Resource*weight));
			var result = baseCollection.Concat(subtract);
			return result.GroupBy(x => x.Skill).Select(y => new SkillEffiencyResource(y.Key, Math.Max(y.Sum(z => z.Resource), 0))).ToArray();
		}

		public void RemoveResource(ActivitySkillsCombination key, SkillCombination skillCombination, double heads,
			double resource, DateTimePeriod? fractionPeriod, DateTimePeriod period)
		{
			if (!_resourceDictionary.ContainsKey(key))
			{
				return;
			}
			var d = _resourceDictionary[key];
			var fractionPeriodResult = d.FractionPeriods;
			if (fractionPeriod.HasValue)
			{
				List<DateTimePeriod> dateTimePeriods = d.FractionPeriods.ToList();
				dateTimePeriods.Remove(fractionPeriod.Value);
				fractionPeriodResult = dateTimePeriods.ToArray();
			}

			var weight = 1d;
			if (fractionPeriod.HasValue)
			{
				weight = (double) fractionPeriod.Value.ElapsedTime().TotalMinutes / period.ElapsedTime().TotalMinutes;
			}

			var res = d.Resource - resource;
			var count = d.Count - heads * weight;
			if (res < 0 || Math.Round(res, 5) == 0.00000d)
			{
				res = 0;
			}

			if (Math.Round(count, 5) == 0.00000d)
			{
				count = 0;
			}

			_resourceDictionary[key] = new InnerPeriodResourceDetail(count, res,
				subtractEffiencyResources(d.EffiencyResources, skillCombination.SkillEfficiencies, weight),
				fractionPeriodResult);
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