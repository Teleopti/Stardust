using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataContainerFromSkillCombinations : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly ConcurrentDictionary<DateTimePeriod, PeriodResource> _dictionary = new ConcurrentDictionary<DateTimePeriod, PeriodResource>();
		private readonly ConcurrentDictionary<DoubleGuidCombinationKey, IEnumerable<ISkill>> _skills = new ConcurrentDictionary<DoubleGuidCombinationKey, IEnumerable<ISkill>>();
		private readonly ILookup<Guid, ISkill> _allSkills;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();
		private readonly IList<ISkill> _allTheSkills;
		public ResourceCalculationDataContainerFromSkillCombinations(List<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> allSkills, bool useAllSkills)
		{
			_allTheSkills = allSkills.ToList();
			_allSkills = _allTheSkills.ToLookup(s => s.Id.GetValueOrDefault());
			PrimarySkillMode = false;
			MinSkillResolution = _allTheSkills.Any() ? _allTheSkills.Min(s => s.DefaultResolution) : 15;
			createDictionary(skillCombinationResources);
			UseAllSkills = useAllSkills;
			_personSkillProvider = new PersonSkillProvider();
		}

		public bool UseAllSkills {get;}

		private void createDictionary(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			foreach (var skillCombinationResource in skillCombinationResources.OrderBy(x => x.StartDateTime))
			{
				var actId = skillCombinationResource.SkillCombination.Select(s => _allSkills[s].First().Activity.Id).First();
				var allSkillsInCombination =
					skillCombinationResource.SkillCombination.Select(s => _allSkills[s].FirstOrDefault())
						.Where(s => s != null)
						.ToArray();

				var skillCombination = new SkillCombination(allSkillsInCombination,skillCombinationResource.Period().ToDateOnlyPeriod(TimeZoneInfo.Utc), new SkillEffiencyResource[] {}, allSkillsInCombination);
				
				var skills = skillCombination.OriginalSkills;
				_skills.TryAdd(skillCombination.MergedKey(), skills);

				var fractionPeriod = new DateTimePeriod(skillCombinationResource.StartDateTime, skillCombinationResource.EndDateTime);

				PeriodResource resource;
				if (_dictionary.TryGetValue(fractionPeriod, out resource))
					resource.AppendResource(new ActivitySkillsCombination(actId.GetValueOrDefault(), skillCombination),
						skillCombination, 0, skillCombinationResource.Resource, fractionPeriod);
				else
				{
					resource = new PeriodResource();
					resource.AppendResource(new ActivitySkillsCombination(actId.GetValueOrDefault(), skillCombination),
						skillCombination, 0, skillCombinationResource.Resource, fractionPeriod);
					_dictionary.TryAdd( fractionPeriod, resource);
				}

			}
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			var skillResolution = MinSkillResolution;
			var aSkill = _allTheSkills.FirstOrDefault(x => x.Activity.Id.GetValueOrDefault().Equals(resourceLayer.PayloadId));
			if (aSkill != null)
				skillResolution = aSkill.DefaultResolution;
			var diff = skillResolution / MinSkillResolution;
			var minute = TimeHelper.FitToDefaultResolutionRoundDown(
				TimeSpan.FromMinutes(resourceLayer.Period.StartDateTime.Minute), skillResolution).Minutes;
			var splittedStart = resourceLayer.Period.StartDateTime;
			var startOnInterval = new DateTime(splittedStart.Year, splittedStart.Month, splittedStart.Day, splittedStart.Hour,
				minute, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startOnInterval, startOnInterval.AddMinutes(skillResolution));
			try
			{
				if (!_dictionary.TryGetValue(period, out PeriodResource resources))
					return;

				var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
				if (skills.Key.Length == 0) return;
				var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);

				resources.RemoveResource(key, skills, 0, resourceLayer.Resource/ diff, resourceLayer.FractionPeriod, resourceLayer.Period);
			}
			catch (ArgumentOutOfRangeException ex) //just to get more info if/when #44525 occurs
			{
				throw new ArgumentOutOfRangeException($"Resources are negative for agent {person.Id.GetValueOrDefault()} on period {resourceLayer.Period}", ex);
			}
		}

		public IDictionary<DoubleGuidCombinationKey, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<DoubleGuidCombinationKey, AffectedSkills>();

			var activityKey = activity.Id.GetValueOrDefault();
			PeriodResource interval;
			if (_dictionary.TryGetValue(periodToCalculate, out interval))
			{
				foreach (var pair in interval.GetSkillKeyResources(activityKey))
				{
					IEnumerable<ISkill> skills;
					if (_skills.TryGetValue(pair.SkillKey, out skills))
					{
						var affectedSkills = skills.ToList();
						if (!UseAllSkills)
							affectedSkills = affectedSkills.Where(s => !s.IsCascading() || s.CascadingIndex == affectedSkills.Min(x => x.CascadingIndex)).ToList();
						AffectedSkills value;
						double previousResource = 0;
						double previousCount = 0;
						var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource );
						if (result.TryGetValue(pair.SkillKey, out value))
						{
							previousResource = value.Resource;
							previousCount = value.Count;
							addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
						}
						value = new AffectedSkills
						{
							Skills = affectedSkills,
							Resource = previousResource + pair.Resource.Resource ,
							Count = previousCount + pair.Resource.Count ,
							SkillEffiencies = accumulatedEffiencies
						};

						result[pair.SkillKey] = value;
					}
				}
				return result;
			}
			var divider = periodToCalculate.ElapsedTime().TotalMinutes / MinSkillResolution;
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			if (periodSplit.Count < 2)
				return result;
			foreach (var dateTimePeriod in periodSplit)
			{
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
							var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource / divider);
							if (result.TryGetValue(pair.SkillKey, out value))
							{
								previousResource = value.Resource;
								previousCount = value.Count;
								addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
							}
							value = new AffectedSkills
							{
								Skills = skills,
								Resource = previousResource + pair.Resource.Resource / divider,
								Count = previousCount + pair.Resource.Count / divider,
								SkillEffiencies = accumulatedEffiencies
							};

							result[pair.SkillKey] = value;
						}
					}
				}
			}
			return result;
		}

		
		public int MinSkillResolution { get; }

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public Tuple<double, double> SkillResources(ISkill skill, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public double ActivityResourcesWhereSeatRequired(ISkill skill, DateTimePeriod period)
		{
			return 0;
		}

		public void AddResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			var skillResolution = MinSkillResolution;
			var aSkill = _allTheSkills.FirstOrDefault(x => x.Activity.Id.GetValueOrDefault().Equals(resourceLayer.PayloadId));
			if (aSkill != null)
				skillResolution = aSkill.DefaultResolution;
			var diff = skillResolution / MinSkillResolution;
			var minute = TimeHelper.FitToDefaultResolutionRoundDown(
				TimeSpan.FromMinutes(resourceLayer.Period.StartDateTime.Minute), skillResolution).Minutes;
			var splittedStart = resourceLayer.Period.StartDateTime;
			var startOnInterval = new DateTime(splittedStart.Year, splittedStart.Month, splittedStart.Day, splittedStart.Hour,
				minute, 0, DateTimeKind.Utc);
			var period = new DateTimePeriod(startOnInterval, startOnInterval.AddMinutes(skillResolution));

			PeriodResource resources = _dictionary.GetOrAdd(period, new PeriodResource());

			var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
			if (skills.Key.Length == 0) return;
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);
			_skills.TryAdd(skills.MergedKey(), skills.Skills);
			//if (resourceLayer.RequiresSeat)
			//{
			//	_activityRequiresSeat.TryAdd(resourceLayer.PayloadId, true);
			//}
			resources.AppendResource(key, skills, 0, resourceLayer.Resource /diff, resourceLayer.FractionPeriod);
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

		public IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public bool PrimarySkillMode { get; }

		public IEnumerable<ExternalStaff> BpoResources => Enumerable.Empty<ExternalStaff>();

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

	}
}