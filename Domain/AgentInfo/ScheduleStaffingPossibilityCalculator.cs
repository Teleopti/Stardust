using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.Repositories;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private const int goodPossibility = 1;
		private const int fairPossibility = 0;
		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ScheduledStaffingProvider _scheduledStaffingProvider;
		private readonly ForecastedStaffingProvider _forecastedStaffingProvider;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IUserTimeZone _timeZone;
		private readonly ISkillDayRepository _skillDayRepository;
		private readonly PersonalSkills _personalSkills = new PersonalSkills();
		private readonly ISupportedSkillsInIntradayProvider _supportedSkillsInIntradayProvider;

		public ScheduleStaffingPossibilityCalculator(INow now, ILoggedOnUser loggedOnUser,
			ScheduledStaffingProvider scheduledStaffingProvider, ForecastedStaffingProvider forecastedStaffingProvider, IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository, IUserTimeZone timeZone, ISkillDayRepository skillDayRepository, ISupportedSkillsInIntradayProvider supportedSkillsInIntradayProvider)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
			_scheduledStaffingProvider = scheduledStaffingProvider;
			_forecastedStaffingProvider = forecastedStaffingProvider;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_timeZone = timeZone;
			_skillDayRepository = skillDayRepository;
			_supportedSkillsInIntradayProvider = supportedSkillsInIntradayProvider;
		}

		public IDictionary<DateTime, int> CalculateIntradayAbsenceIntervalPossibilities()
		{
			var possibilities = CalculateIntradayAbsenceIntervalPossibilities(getTodayDateOnlyPeriod());
			return possibilities.Any() ? possibilities.First().Value : new Dictionary<DateTime, int>();
		}

		public IDictionary<DateOnly, IDictionary<DateTime, int>> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { _loggedOnUser.CurrentUser() },
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_scenarioRepository.Current());
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(period, useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasUnderstaffing(skill);
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		public IDictionary<DateTime, int> CalculateIntradayOvertimeIntervalPossibilities()
		{
			var possibilities = CalculateIntradayOvertimeIntervalPossibilities(getTodayDateOnlyPeriod());
			return possibilities.Any() ? possibilities.First().Value : new Dictionary<DateTime, int>();
		}

		public IDictionary<DateOnly, IDictionary<DateTime, int>> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { _loggedOnUser.CurrentUser() },
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				_scenarioRepository.Current());
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(period, useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasSeriousUnderstaffing(skill);
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		private IEnumerable<skillStaffingData> getSkillStaffingData(DateOnlyPeriod period, bool useShrinkage)
		{
			var personSkills = getSupportedPersonSkills(period);

			var skillStaffingList = new List<skillStaffingData>();
			if (!personSkills.Any()) return skillStaffingList;

			var resolution = personSkills.Min(s => s.Skill.DefaultResolution);
			foreach (var skill in personSkills)
			{
				var skillDays = _skillDayRepository.FindReadOnlyRange(period.Inflate(1), new[] {skill.Skill},
					_scenarioRepository.Current());
				var staffings =
					period.DayCollection()
						.Select(
							p =>
								new
								{
									Date = p,
									Staffing = _scheduledStaffingProvider.StaffingPerSkill(new[] {skill.Skill}, resolution, p, useShrinkage).ToLookup(x => x.StartDateTime),
									Forecasted =
									_forecastedStaffingProvider.StaffingPerSkill(new[] {skill.Skill}, skillDays, resolution, p, useShrinkage).ToLookup(x => x.StartTime)
								});
				var intradayStaffingModels = from s in staffings
					let p = s.Date
					let times = s.Staffing.Select(t => t.Key).Union(s.Forecasted.Select(t => t.Key)).Distinct().OrderBy(t => t).ToArray()
					let staffingSeries = times.Select(t => s.Staffing[t].FirstOrDefault())
					let forecastingSeries = times.Select(t => s.Forecasted[t].FirstOrDefault())
					select new skillStaffingData
					{
						Date = p,
						Skill = skill.Skill,
						Time = times,
						ForecastedStaffing = forecastingSeries.Select(t => t?.Agents).ToArray(),
						ScheduledStaffing = staffingSeries.Select(t => t.StartDateTime == new DateTime() ? null : (double?)t.StaffingLevel).ToArray()
					};

				skillStaffingList.AddRange(intradayStaffingModels);
			}

			return skillStaffingList;
		}

		private IEnumerable<IPersonSkill> getSupportedPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriods(period).ToArray();
			if (!personPeriod.Any())
				return new IPersonSkill[] {};

			var personSkills = personPeriod.SelectMany(p => _personalSkills.PersonSkills(p))
				.Where(p => _supportedSkillsInIntradayProvider.CheckSupportedSkill(p.Skill)).ToArray();

			return !personSkills.Any() ? new IPersonSkill[] {} : personSkills.Distinct();
		}

		private static Dictionary<DateTime, int> calcuateIntervalPossibilities(IEnumerable<skillStaffingData> skillStaffingDatas,
		Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				if (!isSkillScheduled(scheduleDictionary, skillStaffingData.Date, skillStaffingData.Skill))
					continue;

				var staffingSpecification = getStaffingSpecification(skillStaffingData.Skill);

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
					
				    var possibility = getPossibility(staffingInterval, staffingSpecification);
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

	    private static int getPossibility(SkillStaffingInterval staffingInterval, ISpecification<IValidatePeriod> staffingSpecification)
	    {
			var isSatisfied = staffingSpecification.IsSatisfiedBy(staffingInterval);
			int possibility;
			if (staffingSpecification.GetType() == typeof(IntervalHasSeriousUnderstaffing))
			{
				possibility = isSatisfied ? goodPossibility : fairPossibility;
			}
			else
			{
				possibility = isSatisfied ? fairPossibility : goodPossibility;
			}
	        return possibility;
	    }

		private static IDictionary<DateOnly, IDictionary<DateTime, int>> calcuateIntervalPossibilitiesForMultipleDays(
			IEnumerable<skillStaffingData> skillStaffingDatas,
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilitiesDictionary = new Dictionary<DateOnly, IDictionary<DateTime, int>>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				intervalPossibilitiesDictionary.Add(skillStaffingDataGroup.Key,
					calcuateIntervalPossibilities(skillStaffingDataGroup, getStaffingSpecification, scheduleDictionary));
			}
			return intervalPossibilitiesDictionary;
		}

		private static bool isSkillScheduled(IScheduleDictionary scheduleDictionary, DateOnly date, ISkill skill)
		{
			var scheduleDays = scheduleDictionary.SchedulesForDay(date).ToList();
			var scheduleDay = scheduleDays.Any() ? scheduleDays.First() : null;
			var personAssignment = scheduleDay?.PersonAssignment();
			if (personAssignment == null || personAssignment.ShiftLayers.IsEmpty())
				return true;

			var scheduledActivities = personAssignment.MainActivities().Select(m => m.Payload).Where(p => p.RequiresSkill);
			return scheduledActivities.Contains(skill.Activity);
		}

		private bool isShrinkageValidatorEnabled()
		{
			var person = _loggedOnUser.CurrentUser();
			if (person.WorkflowControlSet?.AbsenceRequestOpenPeriods == null)
				return false;

			var timeZone = person.PermissionInformation.DefaultTimeZone();
			return person.WorkflowControlSet.IsAbsenceRequestValidatorEnabled<StaffingThresholdWithShrinkageValidator>(timeZone);
		}

		private static bool hasFairPossibilityInThisInterval(Dictionary<DateTime, int> intervalPossibilities, DateTime time)
		{
			int possibility;
			return intervalPossibilities.TryGetValue(time, out possibility) && possibility == fairPossibility;
		}

		private static bool staffingDataHasValue(skillStaffingData skillStaffingData, int i)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.Length > i &&
												   skillStaffingData.ScheduledStaffing[i].HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.Length > i &&
													skillStaffingData.ForecastedStaffing[i].HasValue;
			return isScheduledStaffingDataAvailable && isForecastedStaffingDataAvailable;
		}

		private DateOnlyPeriod getTodayDateOnlyPeriod()
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			return new DateOnly(usersNow).ToDateOnlyPeriod();
		}

		private class skillStaffingData
		{
			public DateOnly Date { get; set; }
			public ISkill Skill { get; set; }
			public DateTime[] Time { get; set; }
			public double?[] ForecastedStaffing { get; set; }
			public double?[] ScheduledStaffing { get; set; }
		}
	}
}
