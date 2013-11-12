using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainer : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IDictionary<DateTimePeriod, PeriodResource> _dictionary = new Dictionary<DateTimePeriod, PeriodResource>();
		private readonly IDictionary<string,IEnumerable<ISkill>> _skills = new Dictionary<string, IEnumerable<ISkill>>();
		private readonly HashSet<Guid> _activityRequiresSeat = new HashSet<Guid>();
		
		private int _minSkillResolution = 60;

		public ResourceCalculationDataContainer(IPersonSkillProvider personSkillProvider)
		{
			_personSkillProvider = personSkillProvider;
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
			PeriodResource resources;
			if (!_dictionary.TryGetValue(resourceLayer.Period, out resources))
			{
				resources = new PeriodResource();
				_dictionary.Add(resourceLayer.Period, resources);
			}

			var skills = _personSkillProvider.SkillsOnPersonDate(person, personDate);
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills).GenerateKey();
			if (!_skills.ContainsKey(skills.Key))
			{
				if (skills.Skills.Any())
				{
					int minResolution = skills.Skills.Min(s => s.DefaultResolution);
					if (minResolution < MinSkillResolution)
					{
						_minSkillResolution = minResolution;
					}
				}
				_skills.Add(skills.Key,skills.Skills);
			}
			if (resourceLayer.RequiresSeat)
			{
				_activityRequiresSeat.Add(resourceLayer.PayloadId);
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

			var skills = _personSkillProvider.SkillsOnPersonDate(person, personDate);
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills).GenerateKey();

			resources.RemoveResource(key, skills, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period)
		{
			var skillKey = skill.Id.GetValueOrDefault();
			var activityKey = string.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault().ToString();
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
			var activityKey = string.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault().ToString();
			}

			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			PeriodResourceDetail result = new PeriodResourceDetail();
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
			var activityKeys = _activityRequiresSeat.Select(a => a.ToString()).ToArray();
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

			var activityKey = activity.Id.GetValueOrDefault().ToString();
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
							if (result.TryGetValue(pair.SkillKey, out value))
							{
								foreach (var skill in value.SkillEffiencies)
								{
									double effiency;
									if (pair.Effiencies.TryGetValue(skill.Key, out effiency))
									{
										pair.Effiencies[skill.Key] = effiency + skill.Value;
									}
									else
									{
										pair.Effiencies.Add(skill.Key, skill.Value);
									}
								}
								value = new AffectedSkills
									{
										Skills = skills,
										Resource = value.Resource + pair.Resource.Resource,
										Count = value.Count + pair.Resource.Count,
										SkillEffiencies = pair.Effiencies
									};
							}
							else
							{
								value = new AffectedSkills { Skills = skills, Resource = pair.Resource.Resource, Count = pair.Resource.Count, SkillEffiencies = pair.Effiencies };
							}
							result[pair.SkillKey] = value;
						}
					}
				}
			}
			return result;
		}
	}
}