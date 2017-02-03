using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class ResourceCalculationContextFactory
	{
		private readonly IPersonSkillProvider _personSkillProvider;
		private readonly ITimeZoneGuard _timeZoneGuard;

		public ResourceCalculationContextFactory(IPersonSkillProvider personSkillProvider, ITimeZoneGuard timeZoneGuard)
		{
			_personSkillProvider = personSkillProvider;
			_timeZoneGuard = timeZoneGuard;
		}

		[Obsolete("Don't use this one. Always specify a period for performance reasons.")]
		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, bool primarySkillMode)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, primarySkillMode, null));
		}

		public IDisposable Create(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, bool primarySkillMode, DateOnlyPeriod period)
		{
			return new ResourceCalculationContext(createResources(scheduleDictionary, allSkills, primarySkillMode, period));
		}

		public IDisposable Create(List<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> allSkills, bool primarySkillMode, DateOnlyPeriod period, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataConatainerFromSkillCombinations(skillCombinationResources, allSkills, primarySkillMode, useAllSkills)));
		}

		private Lazy<IResourceCalculationDataContainerWithSingleOperation> createResources(IScheduleDictionary scheduleDictionary, IEnumerable<ISkill> allSkills, bool primarySkillMode, DateOnlyPeriod? period)
		{
			var createResources = new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() =>
			{
				var minutesPerInterval = allSkills.Any() ?
					allSkills.Min(s => s.DefaultResolution)
					: 15;
				var extractor = new ScheduleProjectionExtractor(_personSkillProvider, minutesPerInterval, primarySkillMode);
				return period.HasValue ?
					extractor.CreateRelevantProjectionList(scheduleDictionary, period.Value.ToDateTimePeriod(_timeZoneGuard.CurrentTimeZone())) :
					extractor.CreateRelevantProjectionList(scheduleDictionary);
			});
			return createResources;
		}
	}

	public class ResourceCalculationDataConatainerFromSkillCombinations : IResourceCalculationDataContainerWithSingleOperation
	{
		private readonly ConcurrentDictionary<Tuple<string, DateTimePeriod>, PeriodResource> _dictionary = new ConcurrentDictionary<Tuple<string, DateTimePeriod>, PeriodResource>();
		private readonly List<SkillCombinationResource> _skillCombinationResources;
		private readonly IEnumerable<ISkill> _allSkills;

		public ResourceCalculationDataConatainerFromSkillCombinations(List<SkillCombinationResource> skillCombinationResources, IEnumerable<ISkill> allSkills, bool primarySkillMode, bool useAllSkills)
		{
			_skillCombinationResources = skillCombinationResources;
			_allSkills = allSkills;
			PrimarySkillMode = primarySkillMode;
			MinSkillResolution = allSkills.Any() ? allSkills.Min(s => s.DefaultResolution): 15;
			createDictionary(skillCombinationResources);
			UseAllSkills = useAllSkills;
		}

		public bool UseAllSkills { get;}

		private void createDictionary(List<SkillCombinationResource> skillCombinationResources)
		{
			foreach (var skillCombinationResource in skillCombinationResources)
			{
				var actId = _allSkills.First(x => x.Id.Equals(skillCombinationResource.SkillCombination.First())).Activity.Id;
				var allSkillsInCombination =
					_allSkills.Where(x => (skillCombinationResource.SkillCombination.Contains(x.Id.Value))).ToArray();
				var skillCombination = new SkillCombination(allSkillsInCombination,new DateOnlyPeriod(new DateOnly(skillCombinationResource.StartDateTime),
						new DateOnly(skillCombinationResource.EndDateTime)), new SkillEffiencyResource[] {}, allSkillsInCombination);
				
				var fractionPeriod = new DateTimePeriod(skillCombinationResource.StartDateTime, skillCombinationResource.EndDateTime);
				var resource = new PeriodResource();
				resource.AppendResource(new ActivitySkillsCombination(actId.Value, skillCombination),skillCombination, 0, skillCombinationResource.Resource,fractionPeriod);
				_dictionary.TryAdd(new Tuple<string, DateTimePeriod>(skillCombination.Key,fractionPeriod ), resource);
			}
		}

		public IDictionary<string, AffectedSkills> AffectedResources(IActivity activity, DateTimePeriod periodToCalculate)
		{

			var result = new Dictionary<string, AffectedSkills>();

			var activityKey = activity.Id.GetValueOrDefault();
			var divider = periodToCalculate.ElapsedTime().TotalMinutes / MinSkillResolution;
			var periodSplit = periodToCalculate.Intervals(TimeSpan.FromMinutes(MinSkillResolution));
			foreach (var dateTimePeriod in periodSplit)
			{
				_skillCombinationResources.ForEach(sc =>
												   {
													   if (!(new DateTimePeriod(sc.StartDateTime, sc.EndDateTime) == periodToCalculate)) return;
													   var skill = _allSkills.Where(x => x.Id == sc.SkillCombination.FirstOrDefault() && x.Activity.Id == activity.Id);
													   if (!skill.Any())
														   return;
													   else
													   {
														   PeriodResource interval;
														   var allSkillsInCombination = _allSkills.Where(x => sc.SkillCombination.Contains(x.Id.Value)).ToArray();
														   var skillCombination = new SkillCombination(allSkillsInCombination, dateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), new SkillEffiencyResource[] { }, allSkillsInCombination);
														   if (_dictionary.TryGetValue(new Tuple<string, DateTimePeriod>(skillCombination.Key, periodToCalculate), out interval))
														   {
															   foreach (var pair in interval.GetSkillKeyResources(activityKey))
															   {
																   var skills = new List<ISkill>();
																   _skillCombinationResources.ForEach(skillcomb =>
																   {
																	   foreach (var skillId in skillcomb.SkillCombination)
																	   {
																		   if (_allSkills.First(x => x.Id == skillId).Activity == activity && skills.All(x => x.Id != skillId))
																			   skills.Add(_allSkills.First(x => x.Id == skillId));
																	   }
																   });
																   if (skills.Any())
																   {
																	   AffectedSkills value;
																	   double previousResource = 0;
																	   double previousCount = 0;
																	   var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource / divider);
																	   if (result.TryGetValue(pair.SkillKey, out value))
																	   {
																		   previousResource = value.Resource;
																		   previousCount = value.Count;
																		   //addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
																	   }
																	   if (UseAllSkills)
																		   value = new AffectedSkills
																		   {
																			   Skills = skills,
																			   Resource = previousResource + pair.Resource.Resource / divider,
																			   Count = previousCount + pair.Resource.Count / divider,
																			   SkillEffiencies = accumulatedEffiencies
																		   };
																	   else
																		   value = new AffectedSkills
																		   {
																			   Skills = skills.Where(s => !s.IsCascading() || s.CascadingIndex == skills.Min(x => x.CascadingIndex)),
																			   Resource = previousResource + pair.Resource.Resource / divider,
																			   Count = previousCount + pair.Resource.Count / divider,
																			   SkillEffiencies = accumulatedEffiencies
																		   };

																	   result[pair.SkillKey] = value;
																   }
															   }
														   }
													   }


												   });

				//PeriodResource interval;
				//if (_dictionary.TryGetValue(new Tuple<string, DateTimePeriod>(), out interval))
				//{
				//	foreach (var pair in interval.GetSkillKeyResources(activityKey))
				//	{
				//		var skills = new List<ISkill>();
				//		_skillCombinationResources.ForEach(skillcomb =>
				//		{
				//			foreach (var skillId in skillcomb.SkillCombination)
				//			{
				//				if(_allSkills.First(x => x.Id == skillId).Activity == activity && skills.All(x => x.Id != skillId))
				//					skills.Add(_allSkills.First(x => x.Id == skillId));
				//			}
				//		});
				//		if (skills.Any())
				//		{
				//			AffectedSkills value;
				//			double previousResource = 0;
				//			double previousCount = 0;
				//			var accumulatedEffiencies = pair.Effiencies.ToDictionary(k => k.Skill, v => v.Resource / divider);
				//			if (result.TryGetValue(pair.SkillKey, out value))
				//			{
				//				previousResource = value.Resource;
				//				previousCount = value.Count;
				//				//addEfficienciesFromSkillCombination(value.SkillEffiencies, accumulatedEffiencies);
				//			}
				//			if (UseAllSkills)
				//				value = new AffectedSkills
				//				{
				//					Skills = skills,
				//					Resource = previousResource + pair.Resource.Resource/divider,
				//					Count = previousCount + pair.Resource.Count/divider,
				//					SkillEffiencies = accumulatedEffiencies
				//				};
				//			else
				//				value = new AffectedSkills
				//				{
				//					Skills = skills.Where(s => !s.IsCascading() || s.CascadingIndex == skills.Min(x => x.CascadingIndex)),
				//					Resource = previousResource + pair.Resource.Resource/divider,
				//					Count = previousCount + pair.Resource.Count/divider,
				//					SkillEffiencies = accumulatedEffiencies
				//				};

				//			result[pair.SkillKey] = value;
				//		}
				//	}
				//}
			}
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
			//do nothing
		}

		public void RemoveResources(IPerson person, DateOnly personDate, ResourceLayer resourceLayer)
		{
			//do nothing
		}

		public IEnumerable<DateTimePeriod> IntraIntervalResources(ISkill skill, DateTimePeriod period)
		{
			throw new NotImplementedException();
		}

		public bool PrimarySkillMode { get; }
	}
}