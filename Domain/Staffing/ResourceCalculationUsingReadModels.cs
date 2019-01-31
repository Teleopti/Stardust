using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.TeamBlock;
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
		
		public IList<SkillStaffingInterval> LoadAndResourceCalculate(IEnumerable<Guid> skillIds, DateTime startOfDayUtc, DateTime endOfDayUtc, bool useShrinkage)
		{
			var skills = _skillRepository.LoadAll().ToList();
			var firstPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc, x.TimeZone)).Min());
			var lastPeriodDateInSkillTimeZone = new DateOnly(skills.Select(x => TimeZoneInfo.ConvertTimeFromUtc(startOfDayUtc.AddDays(1), x.TimeZone)).Max());
			var dateOnlyPeriod = new DateOnlyPeriod(firstPeriodDateInSkillTimeZone, lastPeriodDateInSkillTimeZone);

			var period = new DateTimePeriod(startOfDayUtc, endOfDayUtc);
			var skillCombinationFetchPeriod = new DateTimePeriod(startOfDayUtc.AddDays(-8), endOfDayUtc);
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(skillCombinationFetchPeriod).ToList();
			
			var skillForecastList =
				_skillForecastReadModelRepository.LoadSkillForecast(skillIds.ToArray(), period);

			var skillStaffPeriods = new List<SkillStaffPeriodEx>();

			var backlogSkillForecastList = skillForecastList.Where(sf => sf.IsBackOffice).ToList();
			var nonBacklogSkillForecastList = skillForecastList.Where(sf => !sf.IsBackOffice).ToList();
			
			foreach (var skillForecast in backlogSkillForecastList)
			{
				var serviceLevel = new ServiceLevel(new Percent(skillForecast.PercentAnswered),
					skillForecast.AnsweredWithinSeconds);
				var serviceAgreement = new ServiceAgreement(serviceLevel, new Percent(0.3), new Percent(0.8));

				var skillStaffPeriod = new SkillStaffPeriodEx(new DateTimePeriod(skillForecast.StartDateTime, skillForecast.EndDateTime), new Task(), serviceAgreement)
				{
					AnsweredWithinSeconds = skillForecast.AnsweredWithinSeconds,
					Forecast = useShrinkage ? skillForecast.AgentsWithShrinkage : skillForecast.Agents,
					SkillId = skillForecast.SkillId,
					Period = new DateTimePeriod(skillForecast.StartDateTime, skillForecast.EndDateTime)
				};
				
				//new DateTimePeriod(skillForecast.StartDateTime, skillForecast.EndDateTime), 
					//new Task(useShrinkage ? skillForecast.AgentsWithShrinkage : skillForecast.Agents,TimeSpan.FromSeconds(skillForecast.AverageHandleTime),TimeSpan.Zero), 
					//serviceAgreement);

				//var skillDayLight = new SkillDayLight(skills.SingleOrDefault(s => s.Id == skillForecast.SkillId));
				//skillStaffPeriod.SetSkillDay(skillDayLight);

				//skillStaffPeriod.ForecastedDistributedDemand
				
				
				//skillStaffPeriod.CalculateStaff();
				skillStaffPeriods.Add(skillStaffPeriod);
			}

			var groupedSkillStaffPeriods = skillStaffPeriods.ToLookup(p => p.SkillId);
			
			foreach (var staffPeriodsForSkillId in groupedSkillStaffPeriods)
			{
				foreach (var intervalForSkillId in staffPeriodsForSkillId)
				{
					intervalForSkillId.CreateSegmentCollection(staffPeriodsForSkillId.ToList());
				}
			}
			
			calculateForecastAgentsForEmailSkills(groupedSkillStaffPeriods,combinationResources);
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

			return skillStaffingIntervals;
		}
		
		private static void calculateForecastAgentsForEmailSkills(ILookup<Guid, SkillStaffPeriodEx> skillStaffPeriodsBySkill, IList<SkillCombinationResource> skillCombinationResources)
		{
			//TODO: move from old structure to new Skill Grouped SkillStaffPeriodEx  
			foreach (var skillStaffPeriodsForSkill in skillStaffPeriodsBySkill)
			{
				var skillId = skillStaffPeriodsForSkill.Key;
				var skillCombinationsForSkill = skillCombinationResources.Where(s => s.SkillCombination.Contains(skillId)).ToList();

				skillStaffPeriodsForSkill.ForEach(ssp => ssp.SetCalculatedResource65(0));
				foreach (var skillStaffPeriodForSkill in skillStaffPeriodsForSkill)
				{
					var totalResources = skillCombinationsForSkill.Where(s => s.StartDateTime == skillStaffPeriodForSkill.Period.StartDateTime)
						.Sum(s => s.Resource);

					if(totalResources > 0)
						skillStaffPeriodForSkill.SetCalculatedResource65(totalResources);
				}
			}
		}
	}
}