using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainerFromStorage : IResourceCalculationDataContainer
	{
		private readonly IDictionary<DateTimePeriod, PeriodResource> _dictionary = new Dictionary<DateTimePeriod, PeriodResource>();
		private readonly IDictionary<string, IEnumerable<ISkill>> _skills = new Dictionary<string, IEnumerable<ISkill>>();
		private readonly HashSet<Guid> _activityRequiresSeat = new HashSet<Guid>();

		public int MinSkillResolution { get; private set; }

		public void Clear()
		{
			_dictionary.Clear();
		}

		public bool HasItems()
		{
			return _dictionary.Count > 0;
		}

		public void AddResources(ResourcesFromStorage resources, int minSkillResolution)
		{
			var command = new TransformResourcesFromStorageCommand(resources);
			command.Execute();

			resources.Transfer(_skills, _activityRequiresSeat, _dictionary);
			MinSkillResolution = minSkillResolution;
		}

		public Tuple<double, double> SkillResources(ISkill skill, DateTimePeriod period)
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
						var resultEffiency = pair.Effiencies.ToDictionary(k => k.Skill,v=>v.Resource);
						IEnumerable<ISkill> skills;
						if (_skills.TryGetValue(pair.SkillKey, out skills))
						{
							AffectedSkills value;
							if (result.TryGetValue(pair.SkillKey, out value))
							{
								foreach (var skill in value.SkillEffiencies)
								{
									double effiency;
									if (resultEffiency.TryGetValue(skill.Key, out effiency))
									{
										resultEffiency[skill.Key] = effiency + skill.Value;
									}
									else
									{
										resultEffiency.Add(skill.Key, skill.Value);
									}
								}
								value = new AffectedSkills
								{
									Skills = skills,
									Resource = value.Resource + pair.Resource.Resource,
									Count = value.Count + pair.Resource.Count,
									SkillEffiencies = resultEffiency
								};
							}
							else
							{
								value = new AffectedSkills { Skills = skills, Resource = pair.Resource.Resource, Count = pair.Resource.Count, SkillEffiencies = resultEffiency};
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