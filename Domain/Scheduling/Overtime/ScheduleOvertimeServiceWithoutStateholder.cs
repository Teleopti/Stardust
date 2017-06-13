using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IScheduleOvertimeServiceWithoutStateholder
	{
		DateTimePeriod? SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences,
			IResourceCalculateDelayerWithoutStateholder resourceCalculateDelayer, DateOnly dateOnly,
			IScheduleTagSetter scheduleTagSetter, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc, DateTimePeriod specifiedPeriod);
	}

	public class ScheduleOvertimeServiceWithoutStateholder : IScheduleOvertimeServiceWithoutStateholder
	{
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IGridlockManager _gridlockManager;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider _personSkillsForScheduleDaysOvertimeProvider;
		private readonly ICalculateBestOvertime _calculateBestOvertime;

		public ScheduleOvertimeServiceWithoutStateholder(ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			IGridlockManager gridlockManager,
			ITimeZoneGuard timeZoneGuard,
			PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider personSkillsForScheduleDaysOvertimeProvider, 
			ICalculateBestOvertime calculateBestOvertime)
		{
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_gridlockManager = gridlockManager;
			_timeZoneGuard = timeZoneGuard;
			_personSkillsForScheduleDaysOvertimeProvider = personSkillsForScheduleDaysOvertimeProvider;
			_calculateBestOvertime = calculateBestOvertime;
		}

		public DateTimePeriod? SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences,
			IResourceCalculateDelayerWithoutStateholder resourceCalculateDelayer, DateOnly dateOnly,
			IScheduleTagSetter scheduleTagSetter, ResourceCalculationData resourceCalculationData, Func<IDisposable> contextFunc, DateTimePeriod requestedPeriod)
		{
			var person = scheduleDay.Person;
			var timeZoneInfo = _timeZoneGuard.CurrentTimeZone();
			if (_gridlockManager.Gridlocks(person, dateOnly) != null)
				return null;
			if (!hasMultiplicatorDefinitionSetOfOvertimeType(person, dateOnly))
				return null;

			var overtimeDuration = new MinMax<TimeSpan>(overtimePreferences.SelectedTimePeriod.StartTime,
				overtimePreferences.SelectedTimePeriod.EndTime);

			var skills = _personSkillsForScheduleDaysOvertimeProvider.Execute(overtimePreferences, person.Period(dateOnly)).ToList();
			if (!skills.Any())
				return null;
			var minResolution = OvertimeLengthDecider.GetMinimumResolution(skills, overtimeDuration,scheduleDay);
			var overtimeSkillIntervalDataAggregatedList = getAggregatedOvertimeSkillIntervals(resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items());
			var overtimeLayerLengthPeriodsUtc = _calculateBestOvertime.GetBestOvertimeInUtc(overtimeDuration, requestedPeriod, scheduleDay,minResolution
												   , overtimePreferences.AvailableAgentsOnly, overtimeSkillIntervalDataAggregatedList);

			var oldRmsValue = calculatePeriodValue(dateOnly, person, timeZoneInfo, resourceCalculationData, skills);
			var rules = setupRules(overtimePreferences);

			foreach (var dateTimePeriod in overtimeLayerLengthPeriodsUtc)
			{
				scheduleDay = scheduleDay.ReFetch();
				scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity, dateTimePeriod, overtimePreferences.OvertimeType);
				_schedulePartModifyAndRollbackService.ClearModificationCollection();
				if (!_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules))
					continue;

				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, resourceCalculationData, contextFunc);
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, resourceCalculationData, contextFunc);

				var newRmsValue = calculatePeriodValue(dateOnly, person, timeZoneInfo, resourceCalculationData, skills);
				if (newRmsValue <= oldRmsValue)
					return dateTimePeriod;

				_schedulePartModifyAndRollbackService.RollbackMinimumChecks();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, resourceCalculationData, contextFunc);
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, resourceCalculationData, contextFunc);
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

		private double calculatePeriodValue(DateOnly dateOnly, IPerson person, TimeZoneInfo timeZoneInfo, ResourceCalculationData resourceCalculationData, IEnumerable<ISkill> skills)
		{
			if (skills.Any())
			{
				var aggregatedSkillStaffPeriods = skillStaffPeriods(skills, dateOnly.Date, timeZoneInfo, resourceCalculationData);
				double? result = SkillDayRootMeanSquare(aggregatedSkillStaffPeriods);
				if (result.HasValue)
					return result.Value;
			}

			return 0;
		}

		private IAggregateSkill createAggregateSkill(IOvertimePreferences overtimePreferences, IPerson person, DateOnly dateOnly)
		{
			var skills = _personSkillsForScheduleDaysOvertimeProvider.Execute(overtimePreferences, person.Period(dateOnly)).ToList();

			if (skills.IsEmpty()) return null;

			var aggregateSkillSkill = new Skill("Agg", "", Color.Pink, 15, skills[0].SkillType);
			aggregateSkillSkill.ClearAggregateSkill();
			foreach (ISkill skill in skills)
			{
				aggregateSkillSkill.AddAggregateSkill(skill);
			}
			aggregateSkillSkill.IsVirtual = true;

			if (aggregateSkillSkill.AggregateSkills.Any())
			{
				((ISkill)aggregateSkillSkill).DefaultResolution = aggregateSkillSkill.AggregateSkills.Min(s => s.DefaultResolution);
			}

			return aggregateSkillSkill;
		}

		private IEnumerable<IOvertimeSkillPeriodData> skillStaffPeriods(IEnumerable<ISkill> skills, DateTime date, TimeZoneInfo timeZoneInfo, ResourceCalculationData resourceCalculationData)
		{
			var intervals = new List<IOvertimeSkillPeriodData>();
			foreach (KeyValuePair<ISkill, IResourceCalculationPeriodDictionary> pair in resourceCalculationData.SkillResourceCalculationPeriodDictionary.Items())
			{
				if (skills.Contains(pair.Key))
				{
					var things =  pair.Value.Items().Select(x => x.Value);
					foreach (var thing in things)
					{
						intervals.Add(new SkillStaffingInterval
						{
							CalculatedResource = ((SkillStaffingInterval) thing).CalculatedResource,
							Forecast = ((SkillStaffingInterval) thing).Forecast,
							StartDateTime = thing.CalculationPeriod.StartDateTime,
							EndDateTime = thing.CalculationPeriod.EndDateTime
						});
					}
				}
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
			return SkillStaffPeriodHelper.CalculateRootMeanSquare(intradayDifferences, 0);
		}
	}
}
