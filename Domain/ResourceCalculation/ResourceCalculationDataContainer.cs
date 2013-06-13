using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainer : IResourceCalculationDataContainer
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly IDictionary<DateTimePeriod,IDictionary<string, double>> _dictionary = new Dictionary<DateTimePeriod, IDictionary<string, double>>();
		private readonly IDictionary<Guid,IActivity> _activities = new Dictionary<Guid, IActivity>(); 
		private readonly IDictionary<string,IEnumerable<ISkill>> _skills = new Dictionary<string, IEnumerable<ISkill>>();
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

		public void AddResources(DateTimePeriod period, Guid activity, IPerson person, DateOnly personDate,
		                         double resource)
		{
			IDictionary<string, double> resources;
			if (!_dictionary.TryGetValue(period, out resources))
			{
				resources = new Dictionary<string, double>();
				_dictionary.Add(period, resources);
			}

			var skills = _personSkillProvider.SkillsOnPersonDate(person, personDate);
			var key = new ActivitySkillsCombination(activity, skills).GenerateKey();
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
			double foundResource;
			if (resources.TryGetValue(key, out foundResource))
			{
				resources[key] = resource + foundResource;
			}
			else
			{
				resources.Add(key, resource);
			}
		}

		public double SkillResources(ISkill skill, DateTimePeriod period)
		{
			var skillKey = skill.Id.GetValueOrDefault().ToString();
			var activityKey = string.Empty;
			if (skill.Activity != null)
			{
				activityKey = skill.Activity.Id.GetValueOrDefault().ToString();
			}

			double resource = 0;
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				IDictionary<string,double> interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				resource += (from pair in interval
				             where
					             (string.IsNullOrEmpty(activityKey) || pair.Key.StartsWith(activityKey)) &&
					             (pair.Key.Contains(skillKey))
				             select pair.Value).Sum();
			}
			return resource;
		}

		public double ActivityResources(Func<IActivity, bool> activitiesToLookFor, ISkill skill, DateTimePeriod period)
		{
			var activities = _activities.Values.Where(activitiesToLookFor);
			var activityKeys = activities.Select(a => a.Id.GetValueOrDefault().ToString()).ToArray();
			var skillKey = skill.Id.GetValueOrDefault().ToString();

			double resource = 0;
			var periodSplit = period.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				IDictionary<string, double> interval;
				if (!_dictionary.TryGetValue(dateTimePeriod, out interval)) continue;

				resource +=
					interval.Where(pair => activityKeys.Any(a => pair.Key.StartsWith(a)) && pair.Key.Contains(skillKey))
					        .Sum(pair => pair.Value);
			}
			return resource;
		}

		public bool AllIsSingleSkill()
		{
			return !_skills.Any(k => k.Key.Contains("_"));
		}

		public  IDictionary<string, Tuple<IEnumerable<ISkill>, double>> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<string, Tuple<IEnumerable<ISkill>, double>>();

			var activityKey = activity.Id.GetValueOrDefault().ToString();
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				IDictionary<string, double> interval;
				if (_dictionary.TryGetValue(dateTimePeriod, out interval))
				{
					foreach (var pair in interval)
					{
						if (!pair.Key.StartsWith(activityKey)) continue;

						var skillKey = pair.Key.Split('|')[1];
						IEnumerable<ISkill> skills;
						if (_skills.TryGetValue(skillKey, out skills))
						{
							Tuple<IEnumerable<ISkill>, double> value;
							value = result.TryGetValue(skillKey, out value)
								        ? new Tuple<IEnumerable<ISkill>, double>(skills, value.Item2 + pair.Value)
								        : new Tuple<IEnumerable<ISkill>, double>(skills, pair.Value);
							result[skillKey] = value;
						}
					}
				}
			}
			return result;
		}
	}
}