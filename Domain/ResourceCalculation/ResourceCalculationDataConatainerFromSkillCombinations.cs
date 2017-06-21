using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationDataConatainerFromSkillCombinations : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly ConcurrentDictionary<Tuple<GuidCombinationKey, DateTimePeriod>, PeriodResource> _dictionary = new ConcurrentDictionary<Tuple<GuidCombinationKey, DateTimePeriod>, PeriodResource>();
		private readonly List<SkillCombinationResource> _skillCombinationResources;
		private readonly ILookup<Guid, ISkill> _allSkills;
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>> _personCombination = new ConcurrentDictionary<IPerson, ConcurrentBag<SkillCombination>>();

		public ResourceCalculationDataConatainerFromSkillCombinations(List<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> allSkills, bool useAllSkills)
		{
			_skillCombinationResources = skillCombinationResources;
			_allSkills = allSkills.ToLookup(s => s.Id.GetValueOrDefault());
			PrimarySkillMode = false;
			MinSkillResolution = allSkills.Any() ? allSkills.Min(s => s.DefaultResolution) : 15;
			createDictionary(skillCombinationResources);
			UseAllSkills = useAllSkills;
			_personSkillProvider = new PersonSkillProvider();
		}

		public bool UseAllSkills {get;}

		private void createDictionary(IEnumerable<SkillCombinationResource> skillCombinationResources)
		{
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				var actId = skillCombinationResource.SkillCombination.Select(s => _allSkills[s].First().Activity.Id).First();
				var allSkillsInCombination = skillCombinationResource.SkillCombination.Select(s => _allSkills[s].FirstOrDefault()).ToArray();

				var skillCombination = new SkillCombination(allSkillsInCombination,skillCombinationResource.Period().ToDateOnlyPeriod(TimeZoneInfo.Utc), new SkillEffiencyResource[] {}, allSkillsInCombination);
				
				var fractionPeriod = new DateTimePeriod(skillCombinationResource.StartDateTime, skillCombinationResource.EndDateTime);
				var resource = new PeriodResource();
				resource.AppendResource(new ActivitySkillsCombination(actId.GetValueOrDefault(), skillCombination),skillCombination, 0, skillCombinationResource.Resource,fractionPeriod);
				_dictionary.TryAdd(new Tuple<GuidCombinationKey, DateTimePeriod>(new GuidCombinationKey(skillCombination.Key),fractionPeriod ), resource);
			}
		}

		public IDictionary<DoubleGuidCombinationKey, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{
			var result = new Dictionary<DoubleGuidCombinationKey, AffectedSkills>();

			var relevantSkillCombinations = _skillCombinationResources.Where(s =>
																				 s.Period().Intersect(periodToCalculate) &&
																				 _allSkills[s.SkillCombination.FirstOrDefault()].FirstOrDefault().Activity == activity);

			relevantSkillCombinations.ForEach(sc =>
											   {
												   PeriodResource interval;
												   var allSkillsInCombination = sc.SkillCombination.Select(s => _allSkills[s].FirstOrDefault()).ToArray();
												   var skillCombination = new SkillCombination(allSkillsInCombination, periodToCalculate.ToDateOnlyPeriod(TimeZoneInfo.Utc), new SkillEffiencyResource[] {}, allSkillsInCombination);
												   if (!_dictionary.TryGetValue(new Tuple<GuidCombinationKey, DateTimePeriod>(new GuidCombinationKey(skillCombination.Key), periodToCalculate), out interval)) return;

												   foreach (var pair in interval.GetSkillKeyResources(activity.Id.GetValueOrDefault()))
												   {
													   AffectedSkills value;
													   double previousResource = 0;
													   double previousCount = 0;
													   var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource);
													   if (result.TryGetValue(pair.SkillKey, out value))
													   {
														   previousResource = value.Resource;
														   previousCount = value.Count;
													   }

													   var affectedSkills = allSkillsInCombination.ToList();
													   if (!UseAllSkills)
														   affectedSkills = allSkillsInCombination.Where(s => !s.IsCascading() || s.CascadingIndex == allSkillsInCombination.Min(x => x.CascadingIndex)).ToList();
													   
														   value = new AffectedSkills
														   {
															   Skills = affectedSkills,
															   Resource = previousResource + pair.Resource.Resource,
															   Count = previousCount + pair.Resource.Count,
															   SkillEffiencies = accumulatedEffiencies
														   };

													   result[pair.SkillKey] = value;
												   }
											   });

			return result;
		}

		public int MinSkillResolution { get; }

		public void Clear()
		{
			throw new NotImplementedException();
		}

		public bool HasItems()
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
			var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
			if (skills.Key.Length == 0) return;
			var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);
			//_skills.TryAdd(skills.MergedKey(), skills.Skills);

			var resources = _dictionary.GetOrAdd(Tuple.Create(new GuidCombinationKey(skills.Key), resourceLayer.Period), new PeriodResource());

			var relevantSkillCombinations = _skillCombinationResources.Where(s =>
				s.Period().StartDateTime.Equals(resourceLayer.Period.StartDateTime) &&
				s.SkillCombination.NonSequenceEquals(skills.Skills.Select(skill => skill.Id.GetValueOrDefault())));

			if (relevantSkillCombinations.Any())
				relevantSkillCombinations.First().Resource += resourceLayer.Resource;
			else
			{
				_skillCombinationResources.Add(new SkillCombinationResource
				{
					StartDateTime = resourceLayer.Period.StartDateTime,
					EndDateTime =  resourceLayer.Period.EndDateTime,
					Resource =  resourceLayer.Resource,
					SkillCombination = skills.Skills.Select(s => s.Id.GetValueOrDefault())
				});
			}
			//if (resourceLayer.RequiresSeat)
			//{
			//	_activityRequiresSeat.TryAdd(resourceLayer.PayloadId, true);
			//}
			resources.AppendResource(key, skills, 0, resourceLayer.Resource, resourceLayer.FractionPeriod);
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			try
			{
				var skills = fetchSkills(person, personDate).ForActivity(resourceLayer.PayloadId);
				if (skills.Key.Length == 0) return;
				var key = new ActivitySkillsCombination(resourceLayer.PayloadId, skills);

				if (!_dictionary.TryGetValue(Tuple.Create(new GuidCombinationKey(skills.Key), resourceLayer.Period), out PeriodResource resources))
					return;

				resources.RemoveResource(key, skills, 0,resourceLayer.Resource, resourceLayer.FractionPeriod);
			}
			catch (ArgumentOutOfRangeException ex) //just to get more info if/when #44525 occurs
			{
				throw new ArgumentOutOfRangeException($"Resources are negative for agent {person.Id.GetValueOrDefault()} on period {resourceLayer.Period}. Resource = {resourceLayer.Resource}", ex);
			}
		}

		public IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public bool PrimarySkillMode { get; }


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