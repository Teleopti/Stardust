using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainer : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IActivityDivider _activityDivider;
		private readonly ConcurrentDictionary<DateTimePeriod, PeriodResource> _dictionary = new ConcurrentDictionary<DateTimePeriod, PeriodResource>();
		private readonly ConcurrentDictionary<DoubleGuidCombinationKey, IEnumerable<ISkill>> _skills = new ConcurrentDictionary<DoubleGuidCombinationKey, IEnumerable<ISkill>>();
		private readonly ConcurrentDictionary<Guid, bool> _activityRequiresSeat = new ConcurrentDictionary<Guid,bool>();
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();
		private const double heads = 1d;

		public ResourceCalculationDataContainer(IEnumerable<ExternalStaff> bpoResources, IPersonSkillProvider personSkillProvider, int minSkillResolution, bool primarySkillMode, IActivityDivider activityDivider)
		{
			_personSkillProvider = personSkillProvider;
			_activityDivider = activityDivider;
			BpoResources = bpoResources;
			MinSkillResolution = minSkillResolution;
			PrimarySkillMode = primarySkillMode;
		}

		public IEnumerable<ExternalStaff> BpoResources { get; }
		public int MinSkillResolution { get; }

		public void Clear()
		{
			_dictionary.Clear();
		}

		public void AddResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			PeriodResource resources = _dictionary.GetOrAdd(resourceLayer.Period, new PeriodResource());

			var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
			if (skills.Key.Length == 0) return;
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);
			_skills.TryAdd(skills.MergedKey(),skills.Skills);
			if (resourceLayer.RequiresSeat)
			{
				_activityRequiresSeat.TryAdd(resourceLayer.PayloadId,true);
			}
			resources.AppendResource(key, skills, heads, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			try
			{
				if (!_dictionary.TryGetValue(resourceLayer.Period, out PeriodResource resources))
					return;

				var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
				if (skills.Key.Length == 0) return;
				var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);

				resources.RemoveResource(key, skills, heads, resourceLayer.Resource, resourceLayer.FractionPeriod, resourceLayer.Period);
			}
			catch (ArgumentOutOfRangeException ex) //just to get more info if/when #44525 occurs
			{
				throw new ArgumentOutOfRangeException($"Resources are negative for agent {person.Id.GetValueOrDefault()} on period {resourceLayer.Period}", ex);
			}
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
				if (!_dictionary.TryGetValue(dateTimePeriod, out var interval)) continue;

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
				if (!_dictionary.TryGetValue(dateTimePeriod, out var interval)) continue;

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
				if (!_dictionary.TryGetValue(dateTimePeriod, out var interval)) continue;

				resource += interval.GetResources(skillKey, activityKeys).Resource;

			}
			return resource / periodSplit.Count;
		}

		public IDictionary<DoubleGuidCombinationKey, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<DoubleGuidCombinationKey, AffectedSkills>();

			var activityKey = activity.Id.GetValueOrDefault();
			var divider = periodToCalculate.ElapsedTime().TotalMinutes/MinSkillResolution;
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(MinSkillResolution));

			var skillsOffset = new Dictionary<int, TimeZoneInfo>();
			foreach (var skillKey in _skills)
			{
				foreach (var skill in skillKey.Value)
				{
					var minutesOffset = skill.TimeZone.BaseUtcOffset.Minutes;
					if(!skillsOffset.ContainsKey(minutesOffset))
						skillsOffset.Add(minutesOffset, skill.TimeZone);
				}
			}

			foreach (var skillOffset in skillsOffset)
			{
				foreach (var dateTimePeriod in periodSplit)
				{
					var adjustedPeriod = _activityDivider.FetchPeriodForSkill(dateTimePeriod, skillOffset.Value);
					if (_dictionary.TryGetValue(adjustedPeriod, out var interval))
					{
						foreach (var pair in interval.GetSkillKeyResources(activityKey))
						{
							if (_skills.TryGetValue(pair.SkillKey, out var skills))
							{
								double previousResource = 0;
								double previousCount = 0;
								var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource/divider);
								if (result.TryGetValue(pair.SkillKey, out var value))
								{
									previousResource = value.Resource;
									previousCount = value.Count;
									addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
								}
								value = new AffectedSkills
								{
									Skills = skills,
									Resource = previousResource + pair.Resource.Resource/divider,
									Count = previousCount + pair.Resource.Count/divider,
									SkillEffiencies = accumulatedEffiencies
								};
							
								result[pair.SkillKey] = value;
							}
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
				if (accumulatedEffiencies.TryGetValue(skill.Key, out var efficiency))
				{
					accumulatedEffiencies[skill.Key] = efficiency + skill.Value;
				}
				else
				{
					accumulatedEffiencies.Add(skill.Key, skill.Value);
				}
			}
		}
	}
}