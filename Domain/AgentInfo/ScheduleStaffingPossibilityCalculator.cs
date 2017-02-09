using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Specification;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private const int goodPossibility = 1;
		private const int fairPossibility = 0;

		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;

		public ScheduleStaffingPossibilityCalculator(INow now, ILoggedOnUser loggedOnUser,
			IStaffingViewModelCreator staffingViewModelCreator, IScheduleStorage scheduleStorage, ICurrentScenario scenarioRepository)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
			_staffingViewModelCreator = staffingViewModelCreator;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
		}

		public IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities()
		{
			var skillStaffingDatas = getSkillStaffingData();

			var periodPossibilities = calcuateIntervalPossibilities(skillStaffingDatas,
				isIntervalUnderstaffed => isIntervalUnderstaffed ? fairPossibility : goodPossibility);

			return periodPossibilities;
		}

		public IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities()
		{
			var skillStaffingDatas = getSkillStaffingData();

			var periodPossibilities = calcuateIntervalPossibilities(skillStaffingDatas,
				isIntervalUnderstaffed => isIntervalUnderstaffed ? goodPossibility : fairPossibility);

			return periodPossibilities;
		}

		private IEnumerable<skillStaffingData> getSkillStaffingData()
		{
			var personSkills = getPersonSkills().ToList();

			return from skill in personSkills.Select(x => x.Skill)
					let intradayStaffingModel = _staffingViewModelCreator.Load(new[] { skill.Id.GetValueOrDefault() })
					select new skillStaffingData
					{
						Skill = skill,
						Time = intradayStaffingModel.DataSeries?.Time,
						ForecastedStaffing = intradayStaffingModel.DataSeries?.ForecastedStaffing,
						ScheduledStaffing = intradayStaffingModel.DataSeries?.ScheduledStaffing
					};
		}

		private IEnumerable<IPersonSkill> getPersonSkills()
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod =
				person.PersonPeriodCollection.FirstOrDefault(x => x.Period.Contains(new DateOnly(_now.UtcDateTime())));
			if (personPeriod == null)
				return new IPersonSkill[] {};
			var personSkills = personPeriod.PersonSkillCollection?.ToList();
			if (personSkills == null || !personSkills.Any())
				return new IPersonSkill[] {};

			var intradaySchedule = getIntradaySchedule();
			var personAssignment = intradaySchedule.PersonAssignment();
			if (personAssignment == null)
				return new IPersonSkill[] {};

			var scheduledActivities = personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill);
			return personSkills.Where(p => scheduledActivities.Contains(p.Skill.Activity));
		}

		private IScheduleDay getIntradaySchedule()
		{
			var date = new DateOnly(_now.UtcDateTime());
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new [] {_loggedOnUser.CurrentUser()},
				new ScheduleDictionaryLoadOptions(false, false),
				new DateOnlyPeriod(date, date),
				defaultScenario);

			return dictionary.SchedulesForDay(date).First();
		}

		private Dictionary<DateTime, int> calcuateIntervalPossibilities(IEnumerable<skillStaffingData> skillStaffingDatas,
			Func<bool, int> getPossiblityByStaffingStatus)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			var useShrinkageValidator = isShrinkageValidatorExists();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				var intervalHasUnderStaffing = useShrinkageValidator
					? (Specification<IValidatePeriod>) new IntervalShrinkageHasUnderstaffing(skillStaffingData.Skill)
					: new IntervalHasUnderstaffing(skillStaffingData.Skill);

				for (var i = 0; i < skillStaffingData.Time?.Length; i++)
				{
					if (!staffingDataHasValue(skillStaffingData, i)) continue;

					var staffingInterval = new SkillStaffingInterval
					{
						CalculatedResource = skillStaffingData.ScheduledStaffing[i].Value,
						FStaff = skillStaffingData.ForecastedStaffing[i].Value
					};

					if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time[i]))
						continue;

					var isIntervalUnderstaffed = intervalHasUnderStaffing.IsSatisfiedBy(staffingInterval);
					var possibility = getPossiblityByStaffingStatus(isIntervalUnderstaffed);
					var key = skillStaffingData.Time[i];
					if (intervalPossibilities.ContainsKey(key))
					{
						intervalPossibilities[key] = possibility;
					}
					else
					{
						intervalPossibilities.Add(key, possibility);
					}
				}
			}
			return intervalPossibilities;
		}

		private bool isShrinkageValidatorExists()
		{
			var person = _loggedOnUser.CurrentUser();
			var isShrinkageValidatorExist = false;
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				return false;

			var openPeriods = person.WorkflowControlSet.AbsenceRequestOpenPeriods;
			var unSorted = openPeriods.Where(
				absenceRequestOpenPeriod =>
					absenceRequestOpenPeriod.StaffingThresholdValidator.GetType() == typeof(StaffingThresholdWithShrinkageValidator));

			var userTimezone = person.PermissionInformation.DefaultTimeZone();

			var today = new DateOnly(_now.UtcDateTime());
			isShrinkageValidatorExist =
				unSorted.Any(
					u =>
						u.OpenForRequestsPeriod.Contains(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone))) &&
						u.GetPeriod(new DateOnly(TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), userTimezone)))
							.Contains(today) &&
						u.AbsenceRequestProcess.GetType() != typeof(DenyAbsenceRequest));

			return isShrinkageValidatorExist;
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			int possibility;
			return intervalPossibilities.TryGetValue(time, out possibility) && possibility == fairPossibility;
		}

		private static bool staffingDataHasValue(skillStaffingData skillStaffingData, int i)
		{
			return skillStaffingData.ScheduledStaffing[i].HasValue && skillStaffingData.ForecastedStaffing[i].HasValue;
		}

		private class skillStaffingData
		{
			public ISkill Skill { get; set; }
			public DateTime[] Time { get; set; }
			public double?[] ForecastedStaffing { get; set; }
			public double?[] ScheduledStaffing { get; set; }
		}
	}
}
