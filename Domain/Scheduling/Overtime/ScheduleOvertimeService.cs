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

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public class ScheduleOvertimeService
	{
		private readonly OvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGridlockManager _gridlockManager;
		private readonly ITimeZoneGuard _timeZoneGuard;
		private readonly PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider _personSkillsForScheduleDaysOvertimeProvider;

		public ScheduleOvertimeService(OvertimeLengthDecider overtimeLengthDecider, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IGridlockManager gridlockManager,
			ITimeZoneGuard timeZoneGuard,
			PersonSkillsUsePrimaryOrAllForScheduleDaysOvertimeProvider personSkillsForScheduleDaysOvertimeProvider)
		{
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_gridlockManager = gridlockManager;
			_timeZoneGuard = timeZoneGuard;
			_personSkillsForScheduleDaysOvertimeProvider = personSkillsForScheduleDaysOvertimeProvider;
		}

		public bool SchedulePersonOnDay(IScheduleRange scheduleRange, IOvertimePreferences overtimePreferences, IResourceCalculateDelayer resourceCalculateDelayer, DateOnly dateOnly, IScheduleTagSetter scheduleTagSetter)
		{
			var person = scheduleRange.Person;
			var timeZoneInfo = _timeZoneGuard.CurrentTimeZone();
			if (_gridlockManager.Gridlocks(person, dateOnly) != null)
				return false;
			if (!hasMultiplierDefinitionSetOfOvertimeType(person, dateOnly, overtimePreferences))
				return false;

			var overtimeDuration = new MinMax<TimeSpan>(overtimePreferences.SelectedTimePeriod.StartTime, overtimePreferences.SelectedTimePeriod.EndTime);
			var overtimeSpecifiedPeriod = new MinMax<TimeSpan>(overtimePreferences.SelectedSpecificTimePeriod.StartTime, overtimePreferences.SelectedSpecificTimePeriod.EndTime);
			var overtimeLayerLengthPeriodsUtc = _overtimeLengthDecider.Decide(overtimePreferences, scheduleRange, dateOnly, overtimeDuration, overtimeSpecifiedPeriod, overtimePreferences.AvailableAgentsOnly);

			var oldRmsValue = calculatePeriodValue(overtimePreferences, dateOnly, person, timeZoneInfo);
			var rules = setupRules(overtimePreferences);

			foreach (var dateTimePeriod in overtimeLayerLengthPeriodsUtc)
			{
				var periodStartMyViewPoint = dateTimePeriod.StartDateTimeLocal(timeZoneInfo);
				var periodEndMyViewPoint = dateTimePeriod.EndDateTimeLocal(timeZoneInfo);
				var overtimeSpecifiedPeriodStartDateTime = dateOnly.Date.Add(overtimeSpecifiedPeriod.Minimum);
				var overtimeSpecifiedPeriodEndDateTime = dateOnly.Date.Add(overtimeSpecifiedPeriod.Maximum);

				if (periodStartMyViewPoint < overtimeSpecifiedPeriodStartDateTime ||
						periodStartMyViewPoint >= overtimeSpecifiedPeriodEndDateTime ||
						periodEndMyViewPoint > overtimeSpecifiedPeriodEndDateTime ||
						periodEndMyViewPoint <= overtimeSpecifiedPeriodStartDateTime)
					continue;

				var scheduleDay = scheduleRange.ScheduledDay(dateOnly);
				scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity, dateTimePeriod, overtimePreferences.OvertimeType);
				_schedulePartModifyAndRollbackService.ClearModificationCollection();
				if (!_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules))
					continue;

				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, false);

				var newRmsValue = calculatePeriodValue(overtimePreferences, dateOnly, person, timeZoneInfo);
				if (newRmsValue <= oldRmsValue)
					return true;

				_schedulePartModifyAndRollbackService.Rollback();
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
				resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, false);
			}

			return false;
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

		private double calculatePeriodValue(IOvertimePreferences overtimePreferences, DateOnly dateOnly, IPerson person, TimeZoneInfo timeZoneInfo)
		{
			var aggregateSkill = createAggregateSkill(overtimePreferences, person, dateOnly);

			if (aggregateSkill != null)
			{
				var aggregatedSkillStaffPeriods = skillStaffPeriods(aggregateSkill, dateOnly.Date, timeZoneInfo);
				double? result = SkillStaffPeriodHelper.SkillDayRootMeanSquare(aggregatedSkillStaffPeriods);
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

		private IEnumerable<ISkillStaffPeriod> skillStaffPeriods(IAggregateSkill aggregateSkill, DateTime date, TimeZoneInfo timeZoneInfo)
		{
			_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date, date.AddDays(1), timeZoneInfo));
			return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date, date.AddDays(1), timeZoneInfo));
		}

		private static bool hasMultiplierDefinitionSetOfOvertimeType(IPerson person, DateOnly dateOnly, IOvertimePreferences overtimePreferences)
		{
			var contract = person.Period(dateOnly)?.PersonContract?.Contract;

			return contract != null 
				   && overtimePreferences?.OvertimeType != null 
				   && overtimePreferences.OvertimeType.MultiplicatorType.Equals(MultiplicatorType.Overtime) 
				   && contract.MultiplicatorDefinitionSetCollection.Contains(overtimePreferences.OvertimeType);
		}
	}
}
