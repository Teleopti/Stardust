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

		public CalculatedPossibilityModel CalculateIntradayAbsenceIntervalPossibilities()
		{
			return CalculateIntradayAbsenceIntervalPossibilities(getTodayDateOnlyPeriod()).FirstOrDefault() ?? new CalculatedPossibilityModel();
		}

		public IList<CalculatedPossibilityModel> CalculateIntradayAbsenceIntervalPossibilities(DateOnlyPeriod period)
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
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15, skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		public CalculatedPossibilityModel CalculateIntradayOvertimeIntervalPossibilities()
		{
			return CalculateIntradayOvertimeIntervalPossibilities(getTodayDateOnlyPeriod()).FirstOrDefault() ?? new CalculatedPossibilityModel();

		}

		public IList<CalculatedPossibilityModel> CalculateIntradayOvertimeIntervalPossibilities(DateOnlyPeriod period)
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
			return calcuateIntervalPossibilitiesForMultipleDays(skillStaffingDatas.FirstOrDefault()?.Resolution ?? 15, skillStaffingDatas, getStaffingSpecification, scheduleDictionary);
		}

		private IEnumerable<skillStaffingData> getSkillStaffingData(DateOnlyPeriod period, bool useShrinkage)
		{
			var personSkills = getSupportedPersonSkills(period);

			var skillStaffingList = new List<skillStaffingData>();
			if (!personSkills.Any()) return skillStaffingList;

			var resolution = personSkills.Min(s => s.Skill.DefaultResolution);

			var skillDays = _skillDayRepository.FindReadOnlyRange(period.Inflate(1), personSkills.Select(s => s.Skill),
				_scenarioRepository.Current()).ToLookup(s => s.Skill);
			foreach (var skill in personSkills)
			{
				var staffings =
					period.DayCollection()
						.Select(
							p =>
								new
								{
									Date = p,
									Staffing = _scheduledStaffingProvider.StaffingPerSkill(new[] {skill.Skill}, resolution, p, useShrinkage).ToLookup(x => x.StartDateTime),
									Forecasted =
									_forecastedStaffingProvider.StaffingPerSkill(new[] {skill.Skill}, skillDays[skill.Skill].ToArray(), resolution, p, useShrinkage).ToLookup(x => x.StartTime)
								});
				var intradayStaffingModels = from s in staffings
					let p = s.Date
					let times =
					s.Staffing.Select(t => t.Key).Union(s.Forecasted.Select(t => t.Key)).Distinct().OrderBy(t => t).ToArray()
					from t in times
					let staffing = s.Staffing[t].FirstOrDefault()
					select new skillStaffingData
					{
						Resolution = resolution,
						Date = p,
						Skill = skill.Skill,
						Time = t,
						ForecastedStaffing = s.Forecasted[t].FirstOrDefault()?.Agents,
						ScheduledStaffing = staffing.StartDateTime == new DateTime() ? null : (double?) staffing.StaffingLevel
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

		private static Dictionary<DateTime, int> calculateIntervalPossibilities(IEnumerable<skillStaffingData> skillStaffingDatas,
		Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification, IScheduleDictionary scheduleDictionary)
		{
			var intervalPossibilities = new Dictionary<DateTime, int>();
			foreach (var skillStaffingData in skillStaffingDatas)
			{
				if (!isSkillScheduled(scheduleDictionary, skillStaffingData.Date, skillStaffingData.Skill))
					continue;

				var staffingSpecification = getStaffingSpecification(skillStaffingData.Skill);
				
					if (!staffingDataHasValue(skillStaffingData)) continue;
					
					if (hasFairPossibilityInThisInterval(intervalPossibilities, skillStaffingData.Time))
						continue;
					
				    var possibility = getPossibility(skillStaffingData, staffingSpecification);
					var key = skillStaffingData.Time;
					if (intervalPossibilities.ContainsKey(key))
					{
						intervalPossibilities[key] = possibility;
					}
					else
					{
						intervalPossibilities.Add(key, possibility);
					}
			}
			return intervalPossibilities;
		}

	    private static int getPossibility(skillStaffingData skillStaffingData, ISpecification<IValidatePeriod> staffingSpecification)
	    {
		    var staffingInterval = new SkillStaffingInterval
		    {
			    CalculatedResource = skillStaffingData.ScheduledStaffing.Value,
			    FStaff = skillStaffingData.ForecastedStaffing.Value
		    };
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

		private static IList<CalculatedPossibilityModel> calcuateIntervalPossibilitiesForMultipleDays(int resolution,
			IEnumerable<skillStaffingData> skillStaffingDatas,
			Func<ISkill, ISpecification<IValidatePeriod>> getStaffingSpecification, IScheduleDictionary scheduleDictionary)
		{
			var calculatedPossibilityModels = new List<CalculatedPossibilityModel>();
			var skillStaffingDataGroups = skillStaffingDatas.GroupBy(s => s.Date);
			foreach (var skillStaffingDataGroup in skillStaffingDataGroups)
			{
				calculatedPossibilityModels.Add(new CalculatedPossibilityModel
				{
					Date = skillStaffingDataGroup.Key,
					IntervalPossibilies = calculateIntervalPossibilities(skillStaffingDataGroup, getStaffingSpecification, scheduleDictionary),
					Resolution = resolution
				});
			}
			return calculatedPossibilityModels;
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

		private static bool staffingDataHasValue(skillStaffingData skillStaffingData)
		{
			var isScheduledStaffingDataAvailable = skillStaffingData.ScheduledStaffing.HasValue;
			var isForecastedStaffingDataAvailable = skillStaffingData.ForecastedStaffing.HasValue;
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
			public DateTime Time { get; set; }
			public double? ForecastedStaffing { get; set; }
			public double? ScheduledStaffing { get; set; }
			public int Resolution { get; set; }
		}
	}
}
