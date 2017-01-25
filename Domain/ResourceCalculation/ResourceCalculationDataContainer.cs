using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainer : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ConcurrentDictionary<DateTimePeriod, PeriodResource> _dictionary = new ConcurrentDictionary<DateTimePeriod, PeriodResource>();
		private readonly ConcurrentDictionary<string, IEnumerable<ISkill>> _skills = new ConcurrentDictionary<string, IEnumerable<ISkill>>();
		private readonly ConcurrentDictionary<Guid, bool> _activityRequiresSeat = new ConcurrentDictionary<Guid,bool>();
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();

		public ResourceCalculationDataContainer(IPersonSkillProvider personSkillProvider, int minSkillResolution, bool primarySkillMode)
		{
			_personSkillProvider = personSkillProvider;
			MinSkillResolution = minSkillResolution;
			PrimarySkillMode = primarySkillMode;
		}

		public int MinSkillResolution { get; }

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool HasItems()
		{
			return _dictionary.Count > 0;
		}

		public void AddResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			PeriodResource resources = _dictionary.GetOrAdd(resourceLayer.Period, new PeriodResource());

			var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
			if (skills.Key == "") return;
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);
			_skills.TryAdd(skills.MergedKey(),skills.Skills);
			if (resourceLayer.RequiresSeat)
			{
				_activityRequiresSeat.TryAdd(resourceLayer.PayloadId,true);
			}
			resources.AppendResource(key, skills, 1d, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			PeriodResource resources;
			if (!_dictionary.TryGetValue(resourceLayer.Period, out resources))
			{
				return;
			}

			var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
			if (skills.Key == "") return;
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);

			resources.RemoveResource(key, skills, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period)
		{
			var skillKey = skill.Id.GetValueOrDefault();
			var activityKey = Guid.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault();
			}

			var result = new List<DateTimePeriod>();
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				PeriodResource interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				var detail = interval.GetFractionResources(activityKey, skillKey);
				result.AddRange(detail);
			}
			return result;
		}

		public bool PrimarySkillMode { get; }

		public Tuple<double,double> SkillResources(ISkill skill, DateTimePeriod period)
		{
			var skillKey = skill.Id.GetValueOrDefault();
			var activityKey = Guid.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault();
			}

			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			var result = new PeriodResourceDetail();
			foreach (var dateTimePeriod in periodSplit)
			{
				PeriodResource interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				var detail = interval.GetResources(skillKey, activityKey);
				result = new PeriodResourceDetail(result.Count + detail.Count, result.Resource + detail.Resource);
			}
			return new Tuple<double, double>(result.Resource / periodSplit.Count, result.Count / periodSplit.Count);
		}

		public double ActivityResourcesWhereSeatRequired(ISkill skill, DateTimePeriod period)
		{
			var activityKeys = _activityRequiresSeat.Keys.ToArray();
			var skillKey = skill.Id.GetValueOrDefault();

			double resource = 0;
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				PeriodResource interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				resource += interval.GetResources(skillKey, activityKeys).Resource;

			}
			return resource / periodSplit.Count;
		}

		public IDictionary<string, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<string, AffectedSkills>();

			var activityKey = activity.Id.GetValueOrDefault();
			var divider = periodToCalculate.ElapsedTime().TotalMinutes/MinSkillResolution;
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				PeriodResource interval;
				if (_dictionary.TryGetValue(dateTimePeriod, out interval))
				{
					foreach (var pair in interval.GetSkillKeyResources(activityKey))
					{
						IEnumerable<ISkill> skills;
						if (_skills.TryGetValue(pair.SkillKey, out skills))
						{
							AffectedSkills value;
							double previousResource = 0;
							double previousCount = 0;
							var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource/divider);
							if (result.TryGetValue(pair.SkillKey, out value))
							{
								previousResource = value.Resource;
								previousCount = value.Count;
								addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
							}
							value = new AffectedSkills
							{
								Skills = skills,
								Resource = previousResource + pair.Resource.Resource/divider,
								Count = previousCount + pair.Resource.Count,
								SkillEffiencies = accumulatedEffiencies
							};
							
							result[pair.SkillKey] = value;
						}
					}
				}
			}
			return result;
		}

		private SkillCombination fetchSkills(IPerson person, DateOnly personDate)
		{
			var foundCombinations = _personCombination.GetOrAdd(person, _ => new ConcurrentBag<SkillCombination>());
			foreach (var foundCombination in foundCombinations.Where(foundCombination => foundCombination.IsValidForDate(personDate)))
			{
				return foundCombination;
			}

			var skillCombination = _personSkillProvider.SkillsOnPersonDate(person, personDate);
			foundCombinations.Add(skillCombination);
			return skillCombination;
		}

		private static void addEfficienciesFromSkillCombination(IDictionary<Guid, double> skillEfficienciesForSkillCombination,
		                                                        IDictionary<Guid, double> accumulatedEffiencies)
		{
			foreach (var skill in skillEfficienciesForSkillCombination)
			{
				double effiency;
				if (accumulatedEffiencies.TryGetValue(skill.Key, out effiency))
				{
					accumulatedEffiencies[skill.Key] = effiency + skill.Value;
				}
				else
				{
					accumulatedEffiencies.Add(skill.Key, skill.Value);
				}
			}
		}
	}
}