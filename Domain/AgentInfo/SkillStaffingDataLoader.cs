using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Staffing;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingDataLoader : ISkillStaffingDataLoader
	{
		private readonly IScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly IForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillDayLoadHelper _skillDayLoadHelper;
		private readonly BacklogSkillTypesForecastCalculator _backlogSkillTypesForecastCalculator;
		private readonly IUserTimeZone _userTimeZone;

		public SkillStaffingDataLoader(
			IScheduledStaffingProvider scheduledStaffingProvider,
			IForecastedStaffingProvider forecastedStaffingProvider,
			ICurrentScenario scenarioRepository, 
			ISkillDayLoadHelper skillDayLoadHelper, 
			BacklogSkillTypesForecastCalculator backlogSkillTypesForecastCalculator, 
			IUserTimeZone userTimeZone
			)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_scenarioRepository = scenarioRepository;
			_skillDayLoadHelper = skillDayLoadHelper;
			_backlogSkillTypesForecastCalculator = backlogSkillTypesForecastCalculator;
			_userTimeZone = userTimeZone;
		}

		public IList<SkillStaffingData> Load(IList<ISkill> skills, DateOnlyPeriod period, bool useShrinkage,
			Func<DateOnly, bool> dayFilter = null)
		{
			var skillStaffingList = new List<SkillStaffingData>();
			if (!skills.Any()) return skillStaffingList;

			var resolution = skills.Min(s => s.DefaultResolution);
			var skillDays = _skillDayLoadHelper
				.LoadSchedulerSkillDaysFlat(period.Inflate(1), skills, _scenarioRepository.Current())
				.ToArray();

			var timeZoneInfo = _userTimeZone.TimeZone();
			foreach (var dateOnly in period.DayCollection())
			{
				var dateOnlyPeriod = dateOnly.ToDateOnlyPeriod();
				var extendedPeriod = dateOnlyPeriod.Inflate(1);
				var skillDaysInDay = skillDays.Where(s => extendedPeriod.Contains(s.CurrentDate)).ToArray();
				var skillSkillDayDictionary = skillDaysInDay.GroupBy(x => x.Skill).ToDictionary(y => y.Key, y => y.AsEnumerable());

				_backlogSkillTypesForecastCalculator.CalculateForecastedAgents(skillSkillDayDictionary, dateOnly,
					timeZoneInfo, useShrinkage);

				var skillStaffingDatas = createSkillStaffingDatas(dateOnlyPeriod, skills, resolution, useShrinkage,
					skillDaysInDay, dayFilter);

				skillStaffingList.AddRange(skillStaffingDatas);
			}

			return skillStaffingList;
		}

		public IList<SkillStaffingData> Load(IList<ISkill> skills, DateOnlyPeriod period, Dictionary<DateOnly, bool> useShrinkageDic, Func<DateOnly, bool> dayFilter = null)
		{
			var skillStaffingList = new List<SkillStaffingData>();
			if (!skills.Any()) return skillStaffingList;

			var resolution = skills.Min(s => s.DefaultResolution);
			var skillDays = _skillDayLoadHelper
				.LoadSchedulerSkillDaysFlat(period.Inflate(1), skills, _scenarioRepository.Current())
				.ToArray();

			var timeZoneInfo = _userTimeZone.TimeZone();
			foreach (var dateOnly in period.DayCollection())
			{
				var dateOnlyPeriod = dateOnly.ToDateOnlyPeriod();
				var extendedPeriod = dateOnlyPeriod.Inflate(1);
				var skillDaysInDay = skillDays.Where(s => extendedPeriod.Contains(s.CurrentDate)).ToArray();
				var skillSkillDayDictionary = skillDaysInDay.GroupBy(x => x.Skill).ToDictionary(y => y.Key, y => y.AsEnumerable());

				var useShrinkage = useShrinkageDic[dateOnly];

				_backlogSkillTypesForecastCalculator.CalculateForecastedAgents(skillSkillDayDictionary, dateOnly, timeZoneInfo, useShrinkage);

				var skillStaffingDatas = createSkillStaffingDatas(dateOnlyPeriod, skills, resolution, useShrinkage,
					skillDaysInDay, dayFilter);

				skillStaffingList.AddRange(skillStaffingDatas);
			}

			return skillStaffingList;
		}

		private IEnumerable<SkillStaffingData> createSkillStaffingDatas(DateOnlyPeriod period, IList<ISkill> skills,
			int resolution, bool useShrinkage, IList<ISkillDay> skillDays, Func<DateOnly, bool> dayFilter)
		{
			var days = period.DayCollection();
			if (dayFilter != null)
			{
				days = days.Where(dayFilter).ToList();
			}

			var dayStaffingDatas = days.Select(day => new
			{
				Date = day,
				Forecasted = getForecastedStaffing(resolution, useShrinkage, skillDays, day)
					.ToLookup(x => (x.StartTime, x.SkillId)),
				Scheduled = getScheduledStaffing(skills, resolution, useShrinkage, day)
					.ToLookup(x => (x.StartDateTime, x.Id ))
			});

			var skillStaffingDatas = from dayStaffingData in dayStaffingDatas
				let scheduledGroupStartTimes = dayStaffingData.Scheduled.Select(t => t.Key.Item1)
				let forecastedGroupStartTimes = dayStaffingData.Forecasted.Select(t => t.Key.Item1)
				let distinctedStartTimes = scheduledGroupStartTimes.Union(forecastedGroupStartTimes).Distinct().OrderBy(t => t)
				from startTime in distinctedStartTimes
				from skill in skills
				let skillTimePair = (startTime, skill.Id.GetValueOrDefault())
				let scheduledStaffing = calculateScheduledStaffing(dayStaffingData.Scheduled[skillTimePair].ToArray())
				let forecastedStaffing = calculateForecastedStaffing(dayStaffingData.Forecasted[skillTimePair].ToArray())
				select new SkillStaffingData
				{
					Resolution = resolution,
					Date = dayStaffingData.Date,
					Skill = skill,
					Time = startTime,
					ScheduledStaffing = scheduledStaffing,
					ForecastedStaffing = forecastedStaffing
				};

			return skillStaffingDatas;
		}

		private static double? calculateForecastedStaffing(IList<StaffingIntervalModel> forecasteds)
		{
			return forecasteds.Any() ? forecasteds.Sum(forecasted => forecasted?.Agents) : null;
		}

		private static double? calculateScheduledStaffing(IList<SkillStaffingIntervalLightModel> scheduleds)
		{
			if (!scheduleds.Any()) return null;
			return scheduleds.Sum(s => s.StartDateTime != default(DateTime) ? (double?) s.StaffingLevel : null);
		}

		private IEnumerable<SkillStaffingIntervalLightModel> getScheduledStaffing(IList<ISkill> skills, int resolution,
			bool useShrinkage, DateOnly day)
		{
			return _scheduledStaffingProvider.StaffingPerSkill(skills, resolution, day, useShrinkage);
		}
		//// The method above should be deleted and replaced by the one below when changes in intraday have been tested properly
		//private IEnumerable<IntradayScheduleStaffingIntervalDTO> getScheduledStaffing(IList<Guid> skillIds, int resolution,bool useShrinkage, DateOnly dayLocal)
		//{
		//	var startOfDayLocal = dayLocal.Date;
		//	return _intradayStaffingApplicationService.GetScheduledStaffing(skillIds.ToArray(), startOfDayLocal, startOfDayLocal.AddDays(1), TimeSpan.FromMinutes(resolution), useShrinkage);
		//}

		private IEnumerable<StaffingIntervalModel> getForecastedStaffing(int resolution, bool useShrinkage,
			IEnumerable<ISkillDay> skillDays, DateOnly day)
		{
			var skillDayDict = skillDays.Where(s => s.CurrentDate >= day.AddDays(-1) && s.CurrentDate <= day.AddDays(1))
				.GroupBy(x => x.Skill).ToDictionary(y => y.Key, y => y.AsEnumerable());
			return _forecastedStaffingProvider.StaffingPerSkill(skillDayDict, resolution, day, useShrinkage);
		}
		//// The method above should be deleted and replaced by the one below when changes in intraday have been tested properly
		//private IEnumerable<IntradayForcastedStaffingIntervalDTO> getForecastedStaffing(IList<Guid> skillIds, int resolution, bool useShrinkage, DateOnly dayLocal)
		//{
		//	var startOfDayLocal = dayLocal.Date;
		//	return _intradayStaffingApplicationService.GetForecastedStaffing(skillIds, startOfDayLocal, startOfDayLocal.AddDays(1), TimeSpan.FromMinutes(resolution), useShrinkage);
		//}
	}
}