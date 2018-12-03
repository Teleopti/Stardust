using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{

	public class ScheduleOvertimeServiceWithoutStateholder
	{
		private readonly SchedulePartModifyAndRollbackServiceWithoutStateHolder _schedulePartModifyAndRollbackService;
		private readonly IGridlockManager _gridlockManager;
		private readonly PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider _personSkillsForScheduleDaysOvertimeProvider;
		private readonly CalculateBestOvertimeBeforeOrAfter _calculateBestOvertime;
		private readonly IResourceCalculation _resourceCalculation;

		public ScheduleOvertimeServiceWithoutStateholder(SchedulePartModifyAndRollbackServiceWithoutStateHolder schedulePartModifyAndRollbackService, 
			IGridlockManager gridlockManager,
			PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider personSkillsForScheduleDaysOvertimeProvider,
			CalculateBestOvertimeBeforeOrAfter calculateBestOvertime, IResourceCalculation resourceCalculation)
		{
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_gridlockManager = gridlockManager;
			_personSkillsForScheduleDaysOvertimeProvider = personSkillsForScheduleDaysOvertimeProvider;
			_calculateBestOvertime = calculateBestOvertime;
			_resourceCalculation = resourceCalculation;
		}

		public DateTimePeriod? SchedulePersonOnDay(IScheduleRange scheduleRange, IOvertimePreferences overtimePreferences, DateOnly dateOnly,
			IScheduleTagSetter scheduleTagSetter, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc, DateTimePeriod requestedPeriod)
		{
			var person = scheduleRange.Person;
			if (_gridlockManager.Gridlocks(person, dateOnly) != null)
				return null;
			if (!hasMultiplicatorDefinitionSetOfOvertimeType(person, dateOnly))
				return null;

			var overtimeDuration = new MinMax<TimeSpan>(overtimePreferences.SelectedTimePeriod.StartTime,
				overtimePreferences.SelectedTimePeriod.EndTime);

			var skills = _personSkillsForScheduleDaysOvertimeProvider.Execute(overtimePreferences, person.Period(dateOnly)).ToList();
			if (!skills.Any())
				return null;
			//var minResolution = OvertimeLengthDecider.GetMinimumResolution(skills.Where(x=>x.Activity.Id.GetValueOrDefault()== overtimePreferences.SkillActivity.Id.GetValueOrDefault()).ToArray(), overtimeDuration, scheduleRange.ScheduledDay(dateOnly));
			//should we use all skills?? or just the currrnet skill resolution
			var minResolution = OvertimeLengthDecider.GetMinimumResolution(skills, overtimeDuration, scheduleRange.ScheduledDay(dateOnly));
			var overtimeSkillIntervalDataAggregatedList =
				getAggregatedOvertimeSkillIntervals(
					resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items()
						.Where(x => x.Key.Activity.Id.GetValueOrDefault() == overtimePreferences.SkillActivity.Id.GetValueOrDefault()));

			var overtimeLayerLengthPeriodsUtc = _calculateBestOvertime.GetBestOvertimeInUtc(overtimeDuration, requestedPeriod, scheduleRange, dateOnly, minResolution
												   , overtimePreferences.AvailableAgentsOnly, overtimeSkillIntervalDataAggregatedList);

			if (!overtimeLayerLengthPeriodsUtc.Any()) return null;

			var oldRmsValue = calculatePeriodValue(resourceCalculationData, skills);
			var rules = setupRules(overtimePreferences);

			foreach (var dateTimePeriod in overtimeLayerLengthPeriodsUtc)
			{
				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
				scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity, dateTimePeriod, overtimePreferences.OvertimeType);
				_schedulePartModifyAndRollbackService.ClearModificationCollection();
				if (!_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules))
					continue;
				
				_resourceCalculation.ResourceCalculate(dateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), resourceCalculationData, contextFunc);

				var newRmsValue = calculatePeriodValue(resourceCalculationData, skills);
				if (newRmsValue <= oldRmsValue)
					return dateTimePeriod;

				_schedulePartModifyAndRollbackService.RollbackMinimumChecks();
				_resourceCalculation.ResourceCalculate(dateTimePeriod.ToDateOnlyPeriod(TimeZoneInfo.Utc), resourceCalculationData, contextFunc);
			}

			return null;
		}

		private IList<IOvertimeSkillIntervalData> getAggregatedOvertimeSkillIntervals(IEnumerable<KeyValuePair<ISkill, IResourceCalculationPeriodDictionary>> dics)
		{
			var overtimePeriods = new List<IOvertimeSkillIntervalData>();

			foreach (var dic in dics)
			{
				foreach (var pair in dic.Value.Items())
				{
					var overtimePeriod = overtimePeriods.FirstOrDefault(x => x.Period == pair.Key);
					if (overtimePeriod != null)
					{
						overtimePeriod.Add(pair.Value.ForecastedDistributedDemand, -((SkillStaffingInterval)pair.Value).AbsoluteDifference);
					}
					else
					{
						overtimePeriods.Add(new OvertimeSkillIntervalData(pair.Key, pair.Value.ForecastedDistributedDemand, -((SkillStaffingInterval)pair.Value).AbsoluteDifference));
					}
				}
			}

			return overtimePeriods;
		}

		private static INewBusinessRuleCollection setupRules(IOvertimePreferences overtimePreferences)
		{
			var rules = NewBusinessRuleCollection.Minimum();
			if (!overtimePreferences.AllowBreakNightlyRest)
			{
				rules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
			}
			if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
			{
				rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			}
			if (!overtimePreferences.AllowBreakWeeklyRest)
			{
				IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				IDayOffMaxFlexCalculator dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
				IEnsureWeeklyRestRule ensureWeeklyRestRule = new EnsureWeeklyRestRule(workTimeStartEndExtractor, dayOffMaxFlexCalculator);
				rules.Add(new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(),
					new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(),
						new VerifyWeeklyRestAroundDayOffSpecification(), ensureWeeklyRestRule)));
			}
			return rules;
		}

		private double calculatePeriodValue(ResourceCalculationData resourceCalculationData, IEnumerable<ISkill> skills)
		{
			if (skills.Any())
			{
				var aggregatedSkillStaffPeriods = skillStaffPeriods(skills, resourceCalculationData);
				double? result = SkillDayRootMeanSquare(aggregatedSkillStaffPeriods);
				if (result.HasValue)
					return result.Value;
			}

			return 0;
		}

		private IEnumerable<IOvertimeSkillPeriodData> skillStaffPeriods(IEnumerable<ISkill> skills, ResourceCalculationData resourceCalculationData)
		{
			var intervals = new List<IOvertimeSkillPeriodData>();
			foreach (var pair in resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items())
			{
				if (!skills.Contains(pair.Key)) continue;

				var things =  pair.Value.Items().Select(x => x.Value);
				intervals.AddRange(things.Select(thing => new SkillStaffingInterval
				{
					CalculatedResource = ((SkillStaffingInterval) thing).CalculatedResource,
					Forecast = ((SkillStaffingInterval) thing).Forecast,
					StartDateTime = thing.CalculationPeriod.StartDateTime,
					EndDateTime = thing.CalculationPeriod.EndDateTime
				}));
			}
			return intervals;
		}

		private static bool hasMultiplicatorDefinitionSetOfOvertimeType(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			if (personPeriod == null) return false;

			var personContract = personPeriod.PersonContract;
			if (personContract == null) return false;

			return personContract.Contract.MultiplicatorDefinitionSetCollection.Any(multiplicatorDefinitionSet => multiplicatorDefinitionSet.MultiplicatorType == MultiplicatorType.Overtime);
		}

		public static double? SkillDayRootMeanSquare(IEnumerable<IOvertimeSkillPeriodData> skillStaffingIntervals)
		{
			var intradayDifferences = skillStaffingIntervals.Select(s => s.AbsoluteDifference * s.CalculationPeriod.ElapsedTime().TotalHours).ToList();
			return SkillStaffPeriodHelper.CalculateRootMeanSquare(intradayDifferences);
		}
	}
}
