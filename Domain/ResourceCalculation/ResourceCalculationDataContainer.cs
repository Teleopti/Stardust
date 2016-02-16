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
		
		private readonly int _minSkillResolution;

		public ResourceCalculationDataContainer(IPersonSkillProvider personSkillProvider, int minSkillResolution)
		{
			_personSkillProvider = personSkillProvider;
			_minSkillResolution = minSkillResolution;
		}

		public int MinSkillResolution
		{
			get { return _minSkillResolution; }
		}

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
			
			var skills = _personSkillProvider.SkillsOnPersonDate(person, personDate);
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);
			_skills.TryAdd(skills.Key,skills.Skills);
			if (resourceLayer.RequiresSeat)
			{
				_activityRequiresSeat.TryAdd(resourceLayer.PayloadId,true);
			}
			resources.AppendResource(key, skills, MinSkillResolution, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			PeriodResource resources;
			if (!_dictionary.TryGetValue(resourceLayer.Period, out resources))
			{
				return;
			}

			var skills = _personSkillProvider.SkillsOnPersonDate(person, personDate);
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

				var detail = interval.GetResources(activityKey, skillKey);
				result = new PeriodResourceDetail(result.Count + detail.Count, result.Resource + detail.Resource);
			}
			return new Tuple<double, double>(result.Resource / periodSplit.Count, result.Count / periodSplit.Count);
		}

		public double ActivityResourcesWhereSeatRequired(ISkill skill, DateTimePeriod period)
		{
			var activityKeys = _activityRequiresSeat.Keys;
			var skillKey = skill.Id.GetValueOrDefault();

			double resource = 0;
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				PeriodResource interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				resource += interval.GetResources(activityKeys, skillKey).Resource;

			}
			return resource / periodSplit.Count;
		}

		public IDictionary<string, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<string, AffectedSkills>();

			var activityKey = activity.Id.GetValueOrDefault();
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
							var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource);
							if (result.TryGetValue(pair.SkillKey, out value))
							{
								previousResource = value.Resource;
								previousCount = value.Count;
								addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
							}
							value = new AffectedSkills
							{
								Skills = skills,
								Resource = previousResource + pair.Resource.Resource,
								Count = previousCount + pair.Resource.Count / MinSkillResolution,
								SkillEffiencies = accumulatedEffiencies
							};
							
							result[pair.SkillKey] = value;
						}
					}
				}
			}
			return result;
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