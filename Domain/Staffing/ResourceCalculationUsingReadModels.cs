using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;

namespace Teleopti.Ccc.Domain.Staffing
{
	public class ResourceCalculationUsingReadModels
	{
		private readonly ISkillRepository _skillRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly ISkillForecastReadModelRepository _skillForecastReadModelRepository;
		private readonly IResourceCalculation _resourceCalculation;

		public ResourceCalculationUsingReadModels(ISkillRepository skillRepository, ISkillCombinationResourceRepository skillCombinationResourceRepository,
			ISkillForecastReadModelRepository skillForecastReadModelRepository, IResourceCalculation resourceCalculation)
		{
			_skillRepository = skillRepository;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillForecastReadModelRepository = skillForecastReadModelRepository;
			_resourceCalculation = resourceCalculation;
		}

		public IList<SkillStaffingInterval> LoadAndResourceCalculate(IEnumerable<Guid> skillIds, DateTime startOfDayUtc, DateTime endOfDayUtc, 
			bool useShrinkage, IUserTimeZone userTimeZone)
		{
			var skills = _skillRepository.LoadAll().ToList();
			var firstPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc, x.TimeZone)).Min());
			var lastPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc.AddDays(1), x.TimeZone)).Max());
			var dateOnlyPeriod = new DateOnlyPeriod(firstPeriodDateInSkillTimeZone, lastPeriodDateInSkillTimeZone);

			var period = new DateTimePeriod(startOfDayUtc, endOfDayUtc);
			var fetchPeriod = new DateTimePeriod(startOfDayUtc.AddDays(-8), endOfDayUtc.AddDays(2));
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(fetchPeriod).ToList();
			
			var skillForecastList =
				_skillForecastReadModelRepository.LoadSkillForecast(skillIds.ToArray(), fetchPeriod);

			var skillStaffPeriods = new List<SkillStaffPeriodForReadmodel>();

			var backlogSkillForecastList = skillForecastList.Where(sf => sf.IsBackOffice).ToList();
			var nonBacklogSkillForecastList = skillForecastList.Where(sf => !sf.IsBackOffice).ToList();

			foreach (var skillForecast in backlogSkillForecastList)
			{
				var serviceLevel = new ServiceLevel(new Percent(skillForecast.PercentAnswered),
					skillForecast.AnsweredWithinSeconds);
				var serviceAgreement = new ServiceAgreement(serviceLevel, new Percent(0.3), new Percent(0.8));
				var dateTimePeriod = new DateTimePeriod(new DateTime(skillForecast.StartDateTime.Ticks, DateTimeKind.Utc),
					new DateTime(skillForecast.EndDateTime.Ticks, DateTimeKind.Utc));
				var skillStaffPeriod = new SkillStaffPeriodForReadmodel(dateTimePeriod, new Task(), serviceAgreement)
				{
					Forecast = useShrinkage ? skillForecast.AgentsWithShrinkage : skillForecast.Agents,
					SkillId = skillForecast.SkillId,
					Period = dateTimePeriod
				};

				skillStaffPeriods.Add(skillStaffPeriod);
			}

			var groupedSkillStaffPeriods = skillStaffPeriods.GroupBy(p => p.SkillId).ToList();

			foreach (var groupedSkillStaffPeriod in groupedSkillStaffPeriods)
			{
				//var groupedPerDay = groupedSkillStaffPeriod.GroupBy(p => TimeZoneHelper.ConvertFromUtc(p.Period.StartDateTime, userTimeZone.TimeZone()).Date).ToList();
				//foreach (var skillStaffPeriodForReadmodels in groupedPerDay.OrderBy(x => x.Key))
				//{
				//	var staffPeriodsFoDay = skillStaffPeriodForReadmodels.OrderBy(x => x.Period.StartDateTime).Cast<ISkillStaffPeriod>().ToList();
				//	foreach (SkillStaffPeriod intervalForSkillId in staffPeriodsFoDay)
				//	{
				//		intervalForSkillId.CreateSkillStaffSegments65(staffPeriodsFoDay, staffPeriodsFoDay.IndexOf(intervalForSkillId));
				//	}
				//}

				//var groupedPerDay = groupedSkillStaffPeriod.GroupBy(p => new DateOnly(TimeZoneHelper.ConvertFromUtc(p.Period.StartDateTime, userTimeZone.TimeZone()).Date)).ToList();
				//foreach (var skillStaffPeriodForReadmodels in groupedPerDay.OrderBy(x => x.Key))
				//{
				//	var ngt = GetSkillStaffPeriodsForDayCalculation(skillStaffPeriods, groupedSkillStaffPeriod.Key,
				//		skillStaffPeriodForReadmodels.Key, skills);

				//	var staffPeriodsFoDay = skillStaffPeriodForReadmodels.OrderBy(x => x.Period.StartDateTime).Cast<ISkillStaffPeriod>().ToList();
				//	foreach (SkillStaffPeriod intervalForSkillId in staffPeriodsFoDay)
				//	{
				//		intervalForSkillId.CreateSkillStaffSegments65(staffPeriodsFoDay, staffPeriodsFoDay.IndexOf(intervalForSkillId));
				//	}
				//}

				


				var staffPeriodsForSkillId = groupedSkillStaffPeriod.Cast<ISkillStaffPeriod>().ToList();
				foreach (SkillStaffPeriod intervalForSkillId in staffPeriodsForSkillId.OrderBy(x => x.Period.StartDateTime))
				{
					intervalForSkillId.CreateSkillStaffSegments65(staffPeriodsForSkillId, staffPeriodsForSkillId.IndexOf(intervalForSkillId));
				}
			}

			calculateForecastAgentsForEmailSkills(groupedSkillStaffPeriods, combinationResources);
			var returnList = new HashSet<SkillStaffingInterval>();
			var intervals = skillStaffPeriods.Select(sspBySkill =>
			{
				var skillStaffPeriod = sspBySkill;
				return new SkillStaffingInterval
				{
					SkillId = skillStaffPeriod.SkillId,
					StartDateTime = skillStaffPeriod.Period.StartDateTime,
					EndDateTime = skillStaffPeriod.Period.EndDateTime,
					Forecast = skillStaffPeriod.FStaff,
					StaffingLevel = skillStaffPeriod.BookedResource65
					//IsBacklogType = true
				};
			}).ToList();
			intervals.ForEach(i => returnList.Add(i));

			var nonBacklogSkillStaffingIntervals = nonBacklogSkillForecastList.Select(skillForecast => new SkillStaffingInterval
			{
				SkillId = skillForecast.SkillId,
				StartDateTime = skillForecast.StartDateTime,
				EndDateTime = skillForecast.EndDateTime,
				Forecast = useShrinkage ? skillForecast.AgentsWithShrinkage : skillForecast.Agents,
				//StaffingLevel = 0,
				//IsBacklogType = false
			}).ToList();
			nonBacklogSkillStaffingIntervals.ForEach(i => returnList.Add(i));

			var skillStaffingIntervals = returnList
				.Where(x => period.Contains(x.StartDateTime) || x.DateTimePeriod.Contains(period.StartDateTime)).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods = skillStaffingIntervals
				.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
				.ToDictionary(k => k.Key,
					v =>
						(IResourceCalculationPeriodDictionary)
						new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
							s => (IResourceCalculationPeriod)s)));

			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));

			using (new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(
				() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, false))))
			{
				_resourceCalculation
					.ResourceCalculate(dateOnlyPeriod, resCalcData,
						() => new ResourceCalculationContext(
							new Lazy<IResourceCalculationDataContainerWithSingleOperation>(
								() => new ResourceCalculationDataContainerFromSkillCombinations(combinationResources, skills, true))));
			}

			var relevantSkillStaffingIntervals = skillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIds.Contains(x.SkillId)).ToList();
			var splitSkillStaffingIntervals = split(relevantSkillStaffingIntervals.ToList(), TimeSpan.FromMinutes(15));
			return splitSkillStaffingIntervals;
		}

		private static void calculateForecastAgentsForEmailSkills(IEnumerable<IGrouping<Guid, SkillStaffPeriodForReadmodel>> skillStaffPeriodsBySkill, IList<SkillCombinationResource> skillCombinationResources)
		{
			foreach (var skillStaffPeriodsForSkill in skillStaffPeriodsBySkill)
			{
				var skillId = skillStaffPeriodsForSkill.Key;
				var skillCombinationsForSkill = skillCombinationResources.Where(s => s.SkillCombination.Contains(skillId)).ToList();

				skillStaffPeriodsForSkill.ForEach(ssp => ssp.SetCalculatedResource65(0));
				foreach (var skillStaffPeriodForSkill in skillStaffPeriodsForSkill)
				{
					var totalResources = skillCombinationsForSkill.Where(s => s.StartDateTime == skillStaffPeriodForSkill.Period.StartDateTime)
						.Sum(s => s.Resource);

					if (totalResources > 0)
						skillStaffPeriodForSkill.SetCalculatedResource65(totalResources);
				}
			}
		}

		private static List<SkillCombinationResource> split(List<SkillCombinationResource> skillCombinationList, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillCombinationResource>();

			foreach (var skillCombinationInterval in skillCombinationList)
			{
				if (!skillCombinationInterval.GetTimeSpan().Equals(resolution))
				{
					var startInterval = skillCombinationInterval.StartDateTime;
					while (startInterval < skillCombinationInterval.EndDateTime)
					{
						dividedIntervals.Add(new SkillCombinationResource
						{
							SkillCombination = skillCombinationInterval.SkillCombination,
							StartDateTime = startInterval,
							EndDateTime = startInterval.Add(resolution),
							Resource = skillCombinationInterval.Resource
						});
						startInterval = startInterval.Add(resolution);
					}
				}
				else
				{
					dividedIntervals.Add(new SkillCombinationResource
					{
						SkillCombination = skillCombinationInterval.SkillCombination,
						StartDateTime = skillCombinationInterval.StartDateTime,
						EndDateTime = skillCombinationInterval.EndDateTime,
						Resource = skillCombinationInterval.Resource
					});
				}
			}
			return dividedIntervals;
		}

		private static SkillStaffingInterval cloneSkillStaffingInterval(SkillStaffingInterval skillStaffingInterval)
		{
			return new SkillStaffingInterval()
			{
				SkillId = skillStaffingInterval.SkillId,
				StartDateTime = skillStaffingInterval.StartDateTime,
				EndDateTime = skillStaffingInterval.EndDateTime,
				StaffingLevel = skillStaffingInterval.StaffingLevel,
				CalculatedResource = skillStaffingInterval.CalculatedResource,
				EstimatedServiceLevel = skillStaffingInterval.EstimatedServiceLevel,
				FStaff = skillStaffingInterval.FStaff,
				Forecast = skillStaffingInterval.Forecast,
				ForecastWithoutShrinkage = skillStaffingInterval.ForecastWithoutShrinkage,
				IsBacklogType = skillStaffingInterval.IsBacklogType,
				Shrinkage = skillStaffingInterval.Shrinkage,
				StaffingLevelWithShrinkage = skillStaffingInterval.StaffingLevelWithShrinkage
			};
		}

		private static IList<SkillStaffingInterval> split(List<SkillStaffingInterval> staffingList, TimeSpan resolution)
		{
			var dividedIntervals = new List<SkillStaffingInterval>();

			foreach (var skillStaffingInterval in staffingList)
			{
				if (!skillStaffingInterval.GetTimeSpan().Equals(resolution))
				{
					var startInterval = skillStaffingInterval.StartDateTime;
					while (startInterval < skillStaffingInterval.EndDateTime)
					{
						var partSkillStaffingInterval = cloneSkillStaffingInterval(skillStaffingInterval);
						partSkillStaffingInterval.StartDateTime = startInterval;
						partSkillStaffingInterval.EndDateTime = startInterval.Add(resolution);
						dividedIntervals.Add(partSkillStaffingInterval);

						startInterval = startInterval.Add(resolution);
					}
				}
				else
				{
					var partSkillStaffingInterval = cloneSkillStaffingInterval(skillStaffingInterval);
					dividedIntervals.Add(partSkillStaffingInterval);
				}
			}
			return dividedIntervals;
		}

		public virtual IEnumerable<SkillStaffPeriodForReadmodel> GetSkillStaffPeriodsForDayCalculation(
			IEnumerable<SkillStaffPeriodForReadmodel> allSkillStaffPeriods, Guid skillId, DateOnly currentDate,
			IList<ISkill> allSkills)
		{
			var foundSkillStaffPeriods = new List<SkillStaffPeriodForReadmodel>();
			var skill = allSkills.SingleOrDefault(x => x.Id.GetValueOrDefault().Equals(skillId));
			//if (ShouldHandleAllTasksWithinSkillStaffPeriod(skillDay))
			//	return skillDay.SkillStaffPeriodCollection;

			var skillResolution = skill.DefaultResolution;
			var endDateTime = currentDate.AddDays(3).Date;
			for (var currentDateTime = TimeZoneHelper.ConvertToUtc(currentDate.Date, skill.TimeZone);
				currentDateTime < endDateTime;
				currentDateTime = currentDateTime.AddMinutes(skillResolution))
			{
				var periods = allSkillStaffPeriods.Where(x => x.Period.StartDateTime == currentDateTime && x.SkillId == skillId);
				if (!periods.Any())
					break;
				//if (!periods.IsAvailable) continue;

				foundSkillStaffPeriods.Add(periods.First());
			}

			return foundSkillStaffPeriods;
		}
	}
}