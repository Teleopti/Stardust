using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Intraday;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.WorkflowControl;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.AgentInfo
{
	public class ScheduleStaffingPossibilityCalculator : IScheduleStaffingPossibilityCalculator
	{
		private const int goodPossibility = 1;
		private const int fairPossibility = 0;
		private const int staffingDataAvailableDays = 13;

		private readonly INow _now;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly ICacheableStaffingViewModelCreator _staffingViewModelCreator;
		private readonly IScheduleStorage _scheduleStorage;
		private readonly ICurrentScenario _scenarioRepository;
		private readonly IUserTimeZone _timeZone;

		public ScheduleStaffingPossibilityCalculator(INow now, ILoggedOnUser loggedOnUser,
			ICacheableStaffingViewModelCreator staffingViewModelCreator, IScheduleStorage scheduleStorage,
			ICurrentScenario scenarioRepository, IUserTimeZone timeZone)
		{
			_now = now;
			_loggedOnUser = loggedOnUser;
			_staffingViewModelCreator = staffingViewModelCreator;
			_scheduleStorage = scheduleStorage;
			_scenarioRepository = scenarioRepository;
			_timeZone = timeZone;
		}

		public IDictionary<DateTime, int> CalcuateIntradayAbsenceIntervalPossibilities()
		{
			var possibilities = CalcuateIntradayAbsenceIntervalPossibilities(getTodayDateOnlyPeriod());
			return possibilities.Any() ? possibilities.First().Value : new Dictionary<DateTime, int>();
		}

		public IDictionary<DateOnly, IDictionary<DateTime, int>> CalcuateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = getScheduleDictionary(period);
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(period, useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasUnderstaffing(skill);
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		public IDictionary<DateTime, int> CalcuateIntradayOvertimeIntervalPossibilities()
		{
			var possibilities = CalcuateIntradayOvertimeIntervalPossibilities(getTodayDateOnlyPeriod());
			return possibilities.Any() ? possibilities.First().Value : new Dictionary<DateTime, int>();
		}

		public IDictionary<DateOnly, IDictionary<DateTime, int>> CalcuateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period)
		{
			var scheduleDictionary = getScheduleDictionary(period);
			var useShrinkageValidator = isShrinkageValidatorEnabled();
			var skillStaffingDatas = getSkillStaffingData(period, useShrinkageValidator);
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification =
				skill => new IntervalHasSeriousUnderstaffing(skill);
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		private IEnumerable<skillStaffingData> getSkillStaffingData(DateOnlyPeriod period, bool useShrinkage)
		{
			var personSkills = getPersonSkills(period);

			var skillStaffingList = new List<skillStaffingData>();
			var staffingDataAvailablePeriod = getStaffingDataAvailablePeriod();
			foreach (var skill in personSkills)
			{
				var intradayStaffingModels =
					_staffingViewModelCreator.Load(skill.Skill.Id.GetValueOrDefault(), staffingDataAvailablePeriod, useShrinkage)
						.Where(x => period.Contains(x.DataSeries.Date))
						.Select(x => new skillStaffingData
						{
							Date = x.DataSeries.Date,
							Skill = skill.Skill,
							Time = x.DataSeries?.Time,
							ForecastedStaffing = x.DataSeries?.ForecastedStaffing,
							ScheduledStaffing = x.DataSeries?.ScheduledStaffing
						});

				skillStaffingList.AddRange(intradayStaffingModels);
			}

			return skillStaffingList;
		}

		private DateOnlyPeriod getStaffingDataAvailablePeriod()
		{
			var usersNow = TimeZoneHelper.ConvertFromUtc(_now.UtcDateTime(), _timeZone.TimeZone());
			var today = new DateOnly(usersNow);
			return new DateOnlyPeriod(today, today.AddDays(staffingDataAvailableDays));
		}

		private IEnumerable<IPersonSkill> getPersonSkills(DateOnlyPeriod period)
		{
			var person = _loggedOnUser.CurrentUser();
			var personPeriod = person.PersonPeriodCollection.Where(x => x.Period.Contains(period)).ToList();
			if (!personPeriod.Any())
				return new IPersonSkill[] { };

			var personSkills = personPeriod.SelectMany(p => p.PersonSkillCollection).ToList();
			return !personSkills.Any() ? new IPersonSkill[] { } : personSkills.Distinct();
		}

		private IScheduleDictionary getScheduleDictionary(DateOnlyPeriod period)
		{
			var defaultScenario = _scenarioRepository.Current();

			var dictionary = _scheduleStorage.FindSchedulesForPersonsOnlyInGivenPeriod(
				new[] { _loggedOnUser.CurrentUser() },
				new ScheduleDictionaryLoadOptions(false, false),
				period,
				defaultScenario);

			return dictionary;
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
