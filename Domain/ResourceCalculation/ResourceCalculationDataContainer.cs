using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainer : IResourceCalculationDataContainer
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IDictionary<DateTimePeriod, PeriodResource> _dictionary = new Dictionary<DateTimePeriod, PeriodResource>();
		private readonly IDictionary<string,IEnumerable<ISkill>> _skills = new Dictionary<string, IEnumerable<ISkill>>();
		private readonly HashSet<Guid> _activityRequiresSeat = new HashSet<Guid>(); 
		
		private int MinSkillResolution = 60;

		public ResourceCalculationDataContainer(IPersonSkillProvider personSkillProvider)
		{
			_personSkillProvider = personSkillProvider;
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
						MinSkillResolution = minResolution;
					}
				}
				_skills.Add(skills.Key,skills.Skills);
			}
			if (resourceLayer.RequiresSeat)
			{
				_activityRequiresSeat.Add(resourceLayer.PayloadId);
			}
			resources.AppendResource(key, skills, resourceLayer.Resource);
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
			return new Tuple<double, double>(result.Resource,result.Count);
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

				resource = Math.Max(resource, interval.GetResources(activityKeys, skillKey).Resource);

			}
			return resource;
		}

		public bool AllIsSingleSkill()
		{
			return !_skills.Any(k => k.Key.Contains("_"));
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

		private class PeriodResource
		{
			private readonly ConcurrentDictionary<string, PeriodResourceDetail> _resourceDictionary = new ConcurrentDictionary<string, PeriodResourceDetail>();
			private readonly ConcurrentDictionary<string, ConcurrentDictionary<Guid, double>> _skillEffiencies = new ConcurrentDictionary<string, ConcurrentDictionary<Guid, double>>();
			private static readonly ConcurrentDictionary<Guid, double> EmptyEfficiencyDictionary = new ConcurrentDictionary<Guid, double>();

			public void AppendResource(string key, SkillCombination skillCombination, double resource)
			{
				_resourceDictionary.AddOrUpdate(key, new PeriodResourceDetail(1, resource), (s, d) => new PeriodResourceDetail(d.Count+1, d.Resource+resource));

				foreach (var skillEfficiency in skillCombination.SkillEfficiencies)
				{
					_skillEffiencies.AddOrUpdate(key,
					                             new ConcurrentDictionary<Guid, double>(new[]
						                             {new KeyValuePair<Guid, double>(skillEfficiency.Key, skillEfficiency.Value)}),
					                             (s, doubles) =>
						                             {
							                             doubles.AddOrUpdate(skillEfficiency.Key, skillEfficiency.Value,
							                                                 (guid, d) => d + skillEfficiency.Value);
							                             return doubles;
						                             });
				}
			}

			public PeriodResourceDetail GetResources(string activityKey, Guid skillKey)
			{
				var count = 0d;
				var resource = 0d;
				foreach (var pair in _resourceDictionary)
				{
					if ((string.IsNullOrEmpty(activityKey) || pair.Key.StartsWith(activityKey)) && (pair.Key.Contains(skillKey.ToString())))
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
				return new PeriodResourceDetail(count,resource);
			}

			public PeriodResourceDetail GetResources(IEnumerable<string> activityKeys, Guid skillKey)
			{
				var count = 0d;
				var resource = 0d;
				foreach (var pair in _resourceDictionary)
				{
					if (activityKeys.Any(a => pair.Key.StartsWith(a)) && pair.Key.Contains(skillKey.ToString()))
					{
						double currentResource = pair.Value.Resource;
						ConcurrentDictionary<Guid, double> effiencies;
						if (_skillEffiencies.TryGetValue(pair.Key, out effiencies))
						{
							double effiency;
							if (effiencies.TryGetValue(skillKey, out effiency))
								currentResource = currentResource*effiency;
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
						effiencies = EmptyEfficiencyDictionary;
					}

					var skillKey = pair.Key.Split('|')[1];
					yield return new SkillKeyResource {SkillKey = skillKey, Resource = pair.Value, Effiencies = effiencies};
				}
			}
		}

		private struct PeriodResourceDetail
		{
			private double _resource;
			private double _count;

			public PeriodResourceDetail(double count, double resource)
			{
				_count = count;
				_resource = resource;
			}

			public double Resource
			{
				get { return _resource; }
				set { _resource = value; }
			}

			public double Count
			{
				get { return _count; }
				set { _count = value; }
			}
		}

		private class SkillKeyResource
		{
			public string SkillKey { get; set; }
			public PeriodResourceDetail Resource { get; set; }

			public IDictionary<Guid, double> Effiencies { get; set; }
		}
	}
}