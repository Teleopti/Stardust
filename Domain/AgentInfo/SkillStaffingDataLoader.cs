using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class SkillStaffingDataLoader : ISkillStaffingDataLoader
	{
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly BacklogSkillTypesForecastCalculator _backlogSkillTypesForecastCalculator;
		private readonly IUserTimeZone _userTimeZone;

		public SkillStaffingDataLoader(ScheduledStaffingProvider scheduledStaffingProvider,
			ForecastedStaffingProvider forecastedStaffingProvider,
			ICurrentScenario scenarioRepository, ISkillDayRepository skillDayRepository, BacklogSkillTypesForecastCalculator backlogSkillTypesForecastCalculator, IUserTimeZone userTimeZone)
		{
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_scenarioRepository = scenarioRepository;
			_skillDayRepository = skillDayRepository;
			_backlogSkillTypesForecastCalculator = backlogSkillTypesForecastCalculator;
			_userTimeZone = userTimeZone;
		}

		public IList<SkillStaffingData> Load(IList<ISkill> skills, DateOnlyPeriod period, bool useShrinkage,
			Func<DateOnly, bool> dayFilter = null)
		{
			var skillStaffingList = new List<SkillStaffingData>();
			if (!skills.Any()) return skillStaffingList;

			var resolution = skills.Min(s => s.DefaultResolution);
			var skillDays = _skillDayRepository.FindReadOnlyRange(period.Inflate(1), skills, _scenarioRepository.Current())
				.ToList();

			var skillDaysBySkills = new Dictionary<ISkill, IEnumerable<ISkillDay>>();
			foreach (var skillDay in skillDays)
			{
				if (!skillDaysBySkills.ContainsKey(skillDay.Skill))
				{
					skillDaysBySkills.Add(skillDay.Skill, new List<ISkillDay>());
				}

				((List<ISkillDay>) skillDaysBySkills[skillDay.Skill]).Add(skillDay);
			}

			_backlogSkillTypesForecastCalculator.CalculateForecastedAgents(period.StartDate, useShrinkage, skillDaysBySkills, _userTimeZone.TimeZone());

			var skillStaffingDatas = createSkillStaffingDatas(period, skills, resolution, useShrinkage,
				skillDays, dayFilter);

			skillStaffingList.AddRange(skillStaffingDatas);
			return skillStaffingList;
		}


		private IEnumerable<SkillStaffingData> createSkillStaffingDatas(DateOnlyPeriod period, IList<ISkill> skills,
			int resolution, bool useShrinkage, IList<ISkillDay> skillDays, Func<DateOnly, bool> dayFilter)
		{
			var days = period.DayCollection().ToList();
			if (dayFilter != null)
			{
				days = days.Where(dayFilter).ToList();
			}

			var dayStaffingDatas = days.Select(day => new
			{
				Date = day,
				Scheduled = getScheduledStaffing(skills, resolution, useShrinkage, day)
					.ToLookup(x => new {StartTime = x.StartDateTime, SkillId = x.Id}),
				Forecasted = getForecastedStaffing(resolution, useShrinkage, skillDays, day)
					.ToLookup(x => new {x.StartTime, x.SkillId})
			});

			var skillStaffingDatas = from dayStaffingData in dayStaffingDatas
				let scheduledGroupStartTimes = dayStaffingData.Scheduled.Select(t => t.Key.StartTime)
				let forecastedGroupStartTimes = dayStaffingData.Forecasted.Select(t => t.Key.StartTime)
				let distinctedStartTimes = scheduledGroupStartTimes.Union(forecastedGroupStartTimes).Distinct().OrderBy(t => t)
				from startTime in distinctedStartTimes
				from skill in skills
				let skillTimePair = new {StartTime = startTime, SkillId = skill.Id.GetValueOrDefault()}
				let scheduledStaffing = calculateScheduledStaffing(dayStaffingData.Scheduled[skillTimePair].ToList())
				let forecastedStaffing = calculateForecastedStaffing(dayStaffingData.Forecasted[skillTimePair].ToList())
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
			return scheduleds.Sum(s => s.StartDateTime != new DateTime() ? (double?) s.StaffingLevel : null);
		}

		private IEnumerable<SkillStaffingIntervalLightModel> getScheduledStaffing(IList<ISkill> skills, int resolution,
			bool useShrinkage, DateOnly day)
		{
			return _scheduledStaffingProvider.StaffingPerSkill(skills, resolution, day, useShrinkage);
		}

		private IEnumerable<StaffingIntervalModel> getForecastedStaffing(int resolution, bool useShrinkage,
			IEnumerable<ISkillDay> skillDays, DateOnly day)
		{
			var skillDayDict = skillDays.Where(s => s.CurrentDate >= day.AddDays(-1) && s.CurrentDate <= day.AddDays(1))
				.ToLookup(x => x.Skill).ToDictionary(y => y.Key, y => y.AsEnumerable());
			return _forecastedStaffingProvider.StaffingPerSkill(skillDayDict, resolution, day, useShrinkage);
		}
	}
}