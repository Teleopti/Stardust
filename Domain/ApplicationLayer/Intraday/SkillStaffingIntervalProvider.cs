﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{

	public class SkillStaffingIntervalProvider : ISkillStaffingIntervalProvider
	{
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public SkillStaffingIntervalProvider(SplitSkillStaffInterval splitSkillStaffInterval,
											 ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, IResourceCalculation resourceCalculation, IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_resourceCalculation = resourceCalculation;
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any())
			{
				return new List<SkillStaffingIntervalLightModel>();
			}

			var skills = _skillRepository.LoadAllSkills().ToList();

			var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(), period.StartDateTime, period.EndDateTime).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
								  v =>
									  (IResourceCalculationPeriodDictionary)
									  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
																							 s => (IResourceCalculationPeriod)s)));
			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
			using (getContext(combinationResources, skills, dateOnlyPeriod, false))
			{
				_resourceCalculation.ResourceCalculate(dateOnlyPeriod, resCalcData, () => getContext(combinationResources, skills, dateOnlyPeriod, true));
			}

			var skillStaffingIntervalsLight = new List<SkillStaffingIntervalLightModel>();

			foreach (var skillStaffingInterval in skillStaffingIntervals)
			{
				if (skillIdList.Contains(skillStaffingInterval.SkillId))
				{
					skillStaffingIntervalsLight.Add(new SkillStaffingIntervalLightModel()
					{
						Id = skillStaffingInterval.SkillId,
						StartDateTime = skillStaffingInterval.StartDateTime,
						EndDateTime = skillStaffingInterval.EndDateTime,
						StaffingLevel = skillStaffingInterval.StaffingLevel
					});
				}

			}

			return skillStaffingIntervalsLight;
			////Fix this
			//var skillCombinationResource = combinationResources.FirstOrDefault();
			//var skillInterval = (int) skillCombinationResource.EndDateTime.Subtract(skillCombinationResource.StartDateTime).TotalMinutes;
			////

			//var skills = _skillRepository.LoadAllSkills();
			//var skillStaffingIntervals = _scheduleForecastSkillReadModelRepository.GetBySkills(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(), period.StartDateTime, period.EndDateTime).ToList();
			//skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);



			//var relevantSkillStaffPeriods =
			//	skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
			//		.ToDictionary(k => k.Key,
			//					  v =>
			//						  (IResourceCalculationPeriodDictionary)
			//						  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
			//																				 s => (IResourceCalculationPeriod) s)));
			//var resourcesForCalculation = new ResourcesExtractorCalculation(combinationResources, skills, skillInterval);
			//var resourcesForShovel = new ResourcesExtractorShovel(combinationResources, skills, skillInterval);


			//var activities = skills.Select(x => x.Activity).Distinct();

			//foreach (var activity in activities)
			//{

			//	//if (!activity.RequiresSkill)
			//	//{
			//	//	var relevantStaffingIntervals = skillStaffingIntervals.Where(s => s.StartDateTime == startDateTime);
			//	//	foreach (var relevantStaffingInterval in relevantStaffingIntervals)
			//	//	{
			//	//		relevantStaffingInterval.ForecastWithShrinkage = 0;
			//	//		relevantStaffingInterval.FStaff = 0;
			//	//	}
			//	//	continue;
			//	//}

			//	var scheduleResourceOptimizer = new ScheduleResourceOptimizer(resourcesForCalculation, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods), new AffectedPersonSkillService(skills), false, new ActivityDivider());
			//	scheduleResourceOptimizer.Optimize(period);

			//	//Shovling
			//	var skillStaffIntervalHolder = new SkillStaffIntervalHolder(relevantSkillStaffPeriods.Select(s =>
			//																								 {
			//																									 IResourceCalculationPeriod ResourceCalculationperiod;
			//																									 s.Value.TryGetValue(period, out ResourceCalculationperiod);
			//																									 return new {s.Key, ResourceCalculationperiod};
			//																								 }).ToDictionary(k => k.Key, v => v.ResourceCalculationperiod));
			//	var cascadingSkills = new CascadingSkills(skills);
			//	if (!cascadingSkills.Any()) continue;

			//	var orderedSkillGroups = _skillGroupPerActivityProvider.FetchOrdered(cascadingSkills, resourcesForShovel, activity, period);
			//	var allSkillGroups = orderedSkillGroups.AllSkillGroups();
			//	foreach (var skillGroupsWithSameIndex in orderedSkillGroups)
			//	{
			//		var state = _primarySkillOverstaff.AvailableSum(skillStaffIntervalHolder, allSkillGroups, skillGroupsWithSameIndex, period);
			//		_addResourcesToSubSkills.Execute(state, skillStaffIntervalHolder, skillGroupsWithSameIndex, period);
			//		_reducePrimarySkillResources.Execute(state, skillStaffIntervalHolder, period);
			//	}
			//}

			//var skillStaffingIntervalsLight = new List<SkillStaffingIntervalLightModel>();

			//foreach (var skillStaffingInterval in skillStaffingIntervals)
			//{
			//	if (skillIdList.Contains(skillStaffingInterval.SkillId))
			//	{
			//		skillStaffingIntervalsLight.Add(new SkillStaffingIntervalLightModel()
			//										{
			//											Id = skillStaffingInterval.SkillId,
			//											StartDateTime = skillStaffingInterval.StartDateTime,
			//											EndDateTime = skillStaffingInterval.EndDateTime,
			//											StaffingLevel = skillStaffingInterval.StaffingLevel
			//										});
			//	}

			//}

			//return skillStaffingIntervalsLight;

			////////////

			//var splittedIntervals = _splitSkillStaffInterval.Split(skillStaffingIntervals.ToList(), resolution);
			//return splittedIntervals
			//	.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime)
			//	.ToList();

		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, DateOnlyPeriod dateOnlyPeriod, bool useAllSkills)
		{
			return new ResourceCalculationContextFactory(null, null).Create(combinationResources, skills, false, dateOnlyPeriod, useAllSkills);
		}
	}



	public class SkillStaffingIntervalProviderOldReadModel : ISkillStaffingIntervalProvider
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;

		public SkillStaffingIntervalProviderOldReadModel(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository, SplitSkillStaffInterval splitSkillStaffInterval)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
			_splitSkillStaffInterval = splitSkillStaffInterval;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution)
		{
			var skillIntervals = _scheduleForecastSkillReadModelRepository.ReadMergedStaffingAndChanges(skillIdList, period);
			var splittedIntervals = _splitSkillStaffInterval.Split(skillIntervals.ToList(), resolution);
			return splittedIntervals
				.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime)
				.ToList();
		}
	}

	public interface ISkillStaffingIntervalProvider
	{
		IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution);
	}
}
