using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourcesFromStorage
	{
		private readonly IEnumerable<ResourcesForCombinationFromStorage> _resourcesForCombination;
		private readonly IEnumerable<ActivitySkillCombinationFromStorage> _activitySkillCombination;
		private readonly IEnumerable<SkillEfficienciesFromStorage> _skillEfficiencies;
		private readonly IEnumerable<ISkill> _allSkills;

		private readonly IDictionary<DateTimePeriod, PeriodResource> _dictionary = new Dictionary<DateTimePeriod, PeriodResource>();
		private readonly IDictionary<string, IEnumerable<ISkill>> _skills = new Dictionary<string, IEnumerable<ISkill>>();
		private readonly HashSet<Guid> _activityRequiresSeat = new HashSet<Guid>();

		public ResourcesFromStorage(IEnumerable<ResourcesForCombinationFromStorage> resourcesForCombination, IEnumerable<ActivitySkillCombinationFromStorage> activitySkillCombination, IEnumerable<SkillEfficienciesFromStorage> skillEfficiencies, IEnumerable<ISkill> allSkills)
		{
			_resourcesForCombination = resourcesForCombination;
			_activitySkillCombination = activitySkillCombination;
			_skillEfficiencies = skillEfficiencies;
			_allSkills = allSkills;
		}

		public void ExtractActivityRequiresSeat()
		{
			var activities = _activitySkillCombination.Where(a => a.ActivityRequiresSeat).Select(a => a.Activity).Distinct();
			foreach (var activity in activities)
			{
				_activityRequiresSeat.Add(activity);
			}
		}

		public void ExtractSkillCombinations()
		{
			var skillCombinationKeys = _activitySkillCombination.Select(a => a.Skills).Distinct();
			foreach (var skillCombinationKey in skillCombinationKeys)
			{
				_skills.Add(skillCombinationKey, skillsFromKey(skillCombinationKey));
			}
		}

		public void ExtractResources()
		{
			var combinationDictionary = _activitySkillCombination.ToDictionary(k => k.Id, v => v);
			foreach (var resourcesForCombinationFromStorage in _resourcesForCombination)
			{
				ActivitySkillCombinationFromStorage combination;
				if (!combinationDictionary.TryGetValue(resourcesForCombinationFromStorage.ActivitySkillCombinationId, out combination)) continue;

				PeriodResource periodResource;
				var period = new DateTimePeriod(DateTime.SpecifyKind(resourcesForCombinationFromStorage.PeriodStart,DateTimeKind.Utc),
												DateTime.SpecifyKind(resourcesForCombinationFromStorage.PeriodEnd, DateTimeKind.Utc));
				if (!_dictionary.TryGetValue(period, out periodResource))
				{
					periodResource = new PeriodResource();
					_dictionary.Add(period, periodResource);
				}

				var foundEfficiencies =
					_skillEfficiencies.Where(s => s.ParentId == resourcesForCombinationFromStorage.Id)
					                  .ToDictionary(k => k.SkillId, v => v.Amount);

				periodResource.AppendResource(combination.Activity.ToString() + "|" + combination.Skills,
											  new SkillCombination(combination.Skills, new ISkill[0], new DateOnlyPeriod(),
											                       foundEfficiencies), resourcesForCombinationFromStorage.Heads, resourcesForCombinationFromStorage.Resources, null);
			}
		}

		private IEnumerable<ISkill> skillsFromKey(string key)
		{
			var idCollection = key.Split('_').Select(i => new Guid(i));
			return _allSkills.Where(s => idCollection.Contains(s.Id.GetValueOrDefault())).ToList();
		}

		public void Clear()
		{
			_activityRequiresSeat.Clear();
			_dictionary.Clear();
			_skills.Clear();
		}

		public void Transfer(IDictionary<string, IEnumerable<ISkill>> skillsTarget,
		                     ICollection<Guid> activityRequiresSeatTarget,
		                     IDictionary<DateTimePeriod, PeriodResource> resourcesTarget)
		{
			skillsTarget.Clear();
			activityRequiresSeatTarget.Clear();
			resourcesTarget.Clear();

			foreach (var keyValuePair in _skills)
			{
				skillsTarget.Add(keyValuePair.Key, keyValuePair.Value);
			}

			foreach (var guid in _activityRequiresSeat)
			{
				activityRequiresSeatTarget.Add(guid);
			}

			foreach (var periodResource in _dictionary)
			{
				resourcesTarget.Add(periodResource.Key,periodResource.Value);
			}
		}
	}
}