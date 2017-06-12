using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ApplicationLayer.Intraday
{
	public class SkillStaffingIntervalProvider
	{
		private readonly SplitSkillStaffInterval _splitSkillStaffInterval;
		private readonly ISkillRepository _skillRepository;
		private readonly IActivityRepository _activityRepository;
		private readonly ISkillCombinationResourceRepository _skillCombinationResourceRepository;
		private readonly IResourceCalculation _resourceCalculation;
		private readonly IExtractSkillForecastIntervals _extractSkillForecastIntervals;

		public SkillStaffingIntervalProvider(SplitSkillStaffInterval splitSkillStaffInterval,
											 ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, IResourceCalculation resourceCalculation, IExtractSkillForecastIntervals extractSkillForecastIntervals, IActivityRepository activityRepository)
		{
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_resourceCalculation = resourceCalculation;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
			_activityRepository = activityRepository;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution, bool useShrinkage)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any())
			{
				return new List<SkillStaffingIntervalLightModel>();
			}

			var allSkillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources, useShrinkage);
			var relevantSkillStaffingIntervals = allSkillStaffingIntervals.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.SkillId)).ToList();
			var splittedIntervals = _splitSkillStaffInterval.Split(relevantSkillStaffingIntervals, resolution, false); 
			return splittedIntervals;

		}

		public List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod period, List<SkillCombinationResource> combinationResources, bool useShrinkage)
		{
			_activityRepository.LoadAll();
			var skills = _skillRepository.LoadAll().ToList();

			var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, period, useShrinkage).ToList();
			skillStaffingIntervals.ForEach(s => s.StaffingLevel = 0);

			var relevantSkillStaffPeriods =
				skillStaffingIntervals.GroupBy(s => skills.First(a => a.Id.GetValueOrDefault() == s.SkillId))
					.ToDictionary(k => k.Key,
								  v =>
									  (IResourceCalculationPeriodDictionary)
									  new ResourceCalculationPeriodDictionary(v.ToDictionary(d => d.DateTimePeriod,
																							 s => (IResourceCalculationPeriod)s)));
			var resCalcData = new ResourceCalculationData(skills, new SlimSkillResourceCalculationPeriodWrapper(relevantSkillStaffPeriods));
			//var dateOnlyPeriod = period.ToDateOnlyPeriod(TimeZoneInfo.Utc);
		    var dateOnlyPeriod = ExtractSkillForecastIntervals.GetLongestPeriod(skills, period);

            using (getContext(combinationResources, skills, false))
			{
				_resourceCalculation.ResourceCalculate(dateOnlyPeriod, resCalcData, () => getContext(combinationResources, skills, true));
			}

			return skillStaffingIntervals;
		}

		private static IDisposable getContext(List<SkillCombinationResource> combinationResources, List<ISkill> skills, bool useAllSkills)
		{
			return new ResourceCalculationContext(new Lazy<IResourceCalculationDataContainerWithSingleOperation>(() => new ResourceCalculationDataConatainerFromSkillCombinations(combinationResources, skills, useAllSkills)));
		}

	
	}


	public class SkillForecastIntervalsFromReadModel : IExtractSkillForecastIntervals
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public SkillForecastIntervalsFromReadModel(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage)
		{
			var skillStaffPeriods = _scheduleForecastSkillReadModelRepository.GetBySkills(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(),
					period.StartDateTime, period.EndDateTime).ToList();
			if (!useShrinkage) return skillStaffPeriods;

			foreach (var skillStaffPeriod in skillStaffPeriods)
			{
				skillStaffPeriod.FStaff = skillStaffPeriod.ForecastWithShrinkage;
			}
			return skillStaffPeriods;

		}
	}

	public class ExtractSkillForecastIntervals : IExtractSkillForecastIntervals
	{
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly ICurrentScenario _currentScenario;

		public ExtractSkillForecastIntervals(ISkillDayRepository skillDayRepository, ICurrentScenario currentScenario)
		{
			_skillDayRepository = skillDayRepository;
			_currentScenario = currentScenario;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage)
		{
			var returnList = new HashSet<SkillStaffingInterval>();
			var skillDays =  _skillDayRepository.FindReadOnlyRange(GetLongestPeriod(skills, period), skills, _currentScenario.Current());
			foreach (var skillDay in skillDays)
			{
				if (useShrinkage)
				{
					foreach (var skillStaffPeriod in skillDay.SkillStaffPeriodCollection)
					{
						skillStaffPeriod.Payload.UseShrinkage = true;
					}
				}
				getSkillStaffingIntervals(skillDay).ForEach(i => returnList.Add(i));
			}
			return returnList.Where(x => period.Contains(x.StartDateTime));
		}

	    public static DateOnlyPeriod GetLongestPeriod(IList<ISkill> skills, DateTimePeriod period)
	    {
	        var returnPeriod = new DateOnlyPeriod(new DateOnly(period.StartDateTime.Date), new DateOnly(period.EndDateTime.Date));
	        foreach (var skill in skills)
	        {
	            var temp = period.ToDateOnlyPeriod(skill.TimeZone);
                if(temp.StartDate < returnPeriod.StartDate)
                    returnPeriod = new DateOnlyPeriod(temp.StartDate, returnPeriod.EndDate);
                if (temp.EndDate > returnPeriod.EndDate)
                    returnPeriod = new DateOnlyPeriod(returnPeriod.StartDate, temp.EndDate);
            }
            return returnPeriod;
	    }
		private IEnumerable<SkillStaffingInterval> getSkillStaffingIntervals(ISkillDay skillDay)
		{
			var skillStaffPeriods = skillDay.SkillStaffPeriodCollection;
			
			return skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
			{
				SkillId = skillDay.Skill.Id.GetValueOrDefault(),
				StartDateTime = skillStaffPeriod.Period.StartDateTime,
				EndDateTime = skillStaffPeriod.Period.EndDateTime,
				Forecast = skillStaffPeriod.FStaff,
				StaffingLevel = 0,
			});
		}
	}

	public interface IExtractSkillForecastIntervals
	{
		IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period, bool useShrinkage);
	}
}
