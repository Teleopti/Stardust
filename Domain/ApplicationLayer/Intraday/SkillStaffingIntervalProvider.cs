using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests;
using Teleopti.Ccc.Domain.Common;
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
		private readonly IExtractSkillForecastIntervals _extractSkillForecastIntervals;

		public SkillStaffingIntervalProvider(SplitSkillStaffInterval splitSkillStaffInterval,
											 ISkillCombinationResourceRepository skillCombinationResourceRepository, ISkillRepository skillRepository, IResourceCalculation resourceCalculation, IExtractSkillForecastIntervals extractSkillForecastIntervals)
		{
			_splitSkillStaffInterval = splitSkillStaffInterval;
			_skillCombinationResourceRepository = skillCombinationResourceRepository;
			_skillRepository = skillRepository;
			_resourceCalculation = resourceCalculation;
			_extractSkillForecastIntervals = extractSkillForecastIntervals;
		}

		public IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution)
		{
			var combinationResources = _skillCombinationResourceRepository.LoadSkillCombinationResources(period).ToList();
			if (!combinationResources.Any())
			{
				return new List<SkillStaffingIntervalLightModel>();
			}

			var skillStaffingIntervals = GetSkillStaffIntervalsAllSkills(period, combinationResources);

			var splittedIntervals = _splitSkillStaffInterval.Split(skillStaffingIntervals.ToList(), resolution);
			return splittedIntervals
				.Where(x => x.StartDateTime >= period.StartDateTime && x.EndDateTime <= period.EndDateTime && skillIdList.Contains(x.Id))
				.ToList();

		}

		public List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod period, List<SkillCombinationResource> combinationResources)
		{
			var skills = _skillRepository.LoadAllSkills().ToList();

			var skillStaffingIntervals = _extractSkillForecastIntervals.GetBySkills(skills, period).ToList();
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

			return skillStaffingIntervals;
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

		public List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod period, List<SkillCombinationResource> combinationResources)
		{
			throw new NotImplementedException();
		}
	}

	public interface ISkillStaffingIntervalProvider
	{
		IList<SkillStaffingIntervalLightModel> StaffingForSkills(Guid[] skillIdList, DateTimePeriod period, TimeSpan resolution);
		List<SkillStaffingInterval> GetSkillStaffIntervalsAllSkills(DateTimePeriod period, List<SkillCombinationResource> combinationResources);
	}


	public class SkillForecastIntervalsFromReadModel : IExtractSkillForecastIntervals
	{
		private readonly IScheduleForecastSkillReadModelRepository _scheduleForecastSkillReadModelRepository;

		public SkillForecastIntervalsFromReadModel(IScheduleForecastSkillReadModelRepository scheduleForecastSkillReadModelRepository)
		{
			_scheduleForecastSkillReadModelRepository = scheduleForecastSkillReadModelRepository;
		}

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period)
		{
			return
				_scheduleForecastSkillReadModelRepository.GetBySkills(skills.Select(x => x.Id.GetValueOrDefault()).ToArray(),
					period.StartDateTime, period.EndDateTime).ToList();
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

		public IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period)
		{
			var returnList = new List<SkillStaffingInterval>();
			var skillDays =  _skillDayRepository.FindReadOnlyRange(period.ToDateOnlyPeriod(TimeZoneInfo.Utc), skills, _currentScenario.Current());
			foreach (var skillDay in skillDays)
			{
				returnList.AddRange(getSkillStaffingIntervals(skillDay));
			}
			return returnList;
		}

		private IEnumerable<SkillStaffingInterval> getSkillStaffingIntervals(ISkillDay skillDay)
		{
			var skillStaffPeriods = skillDay.CompleteSkillStaffPeriodCollection;
			
			return skillStaffPeriods.Select(skillStaffPeriod => new SkillStaffingInterval
			{
				SkillId = skillDay.Skill.Id.GetValueOrDefault(),
				StartDateTime = skillStaffPeriod.Period.StartDateTime,
				EndDateTime = skillStaffPeriod.Period.EndDateTime,
				Forecast = skillStaffPeriod.FStaff,
				ForecastWithShrinkage = skillStaffPeriod.ForecastedDistributedDemandWithShrinkage,
				StaffingLevel = 0,
			});
		}
	}

	public interface IExtractSkillForecastIntervals
	{
		IEnumerable<SkillStaffingInterval> GetBySkills(IList<ISkill> skills, DateTimePeriod period);
	}
}
