﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Legacy.Commands;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
	public interface IScheduleOvertimeService
	{
		bool SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences,
			IResourceCalculateDelayer resourceCalculateDelayer, DateOnly dateOnly, INewBusinessRuleCollection rules,
			IScheduleTagSetter scheduleTagSetter, TimeZoneInfo timeZoneInfo);
	}

	public class ScheduleOvertimeService : IScheduleOvertimeService
	{
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IGridlockManager _gridlockManager;

		public ScheduleOvertimeService(IOvertimeLengthDecider overtimeLengthDecider, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
			ISchedulingResultStateHolder schedulingResultStateHolder,
			IGridlockManager gridlockManager )
		{
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_gridlockManager = gridlockManager;
		}

		public bool SchedulePersonOnDay(IScheduleDay scheduleDay, IOvertimePreferences overtimePreferences, IResourceCalculateDelayer resourceCalculateDelayer, DateOnly dateOnly, INewBusinessRuleCollection rules, IScheduleTagSetter scheduleTagSetter, TimeZoneInfo timeZoneInfo)
		{
			var person = scheduleDay.Person;
			if (_gridlockManager.Gridlocks(person, dateOnly) != null) return false;
			if (!hasMultiplicatorDefinitionSetOfOvertimeType(person, dateOnly)) return false;

			var overtimeDuration = new MinMax<TimeSpan>(overtimePreferences.SelectedTimePeriod.StartTime, overtimePreferences.SelectedTimePeriod.EndTime);
			var overtimeSpecifiedPeriod = new MinMax<TimeSpan>(overtimePreferences.SelectedSpecificTimePeriod.StartTime, overtimePreferences.SelectedSpecificTimePeriod.EndTime);
			var overtimeLayerLengthPeriodsUtc = _overtimeLengthDecider.Decide(person, dateOnly, scheduleDay, overtimePreferences.SkillActivity, overtimeDuration, overtimeSpecifiedPeriod, overtimePreferences.AvailableAgentsOnly);

			var overtimeLayerLengthPeriods = new List<DateTimePeriod>();
			foreach (var dateTimePeriod in overtimeLayerLengthPeriodsUtc)
			{
				var periodStartMyViewPoint = dateTimePeriod.StartDateTimeLocal(timeZoneInfo);
				var periodEndMyViewPoint = dateTimePeriod.EndDateTimeLocal(timeZoneInfo);
				var overtimeSpecifiedPeriodStartDateTime = dateOnly.Date.Add(overtimeSpecifiedPeriod.Minimum);
				var overtimeSpecifiedPeriodEndDateTime = dateOnly.Date.Add(overtimeSpecifiedPeriod.Maximum);
				bool startOk = periodStartMyViewPoint >= overtimeSpecifiedPeriodStartDateTime &&
					periodStartMyViewPoint < overtimeSpecifiedPeriodEndDateTime;
				bool endOk = periodEndMyViewPoint <= overtimeSpecifiedPeriodEndDateTime &&
							 periodEndMyViewPoint > overtimeSpecifiedPeriodStartDateTime;
				if(startOk && endOk)
					overtimeLayerLengthPeriods.Add(dateTimePeriod);
			}

			if (overtimeLayerLengthPeriods.Count == 0) return false;

			var oldRmsValue = calculatePeriodValue(dateOnly, overtimePreferences.SkillActivity, person, timeZoneInfo);

			foreach (var overtimeLayerLengthPeriod in overtimeLayerLengthPeriods)
			{
				scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity, overtimeLayerLengthPeriod, overtimePreferences.OvertimeType);
			}

			if (!overtimePreferences.AllowBreakNightlyRest) rules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
			if (!overtimePreferences.AllowBreakMaxWorkPerWeek) rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
			if (!overtimePreferences.AllowBreakWeeklyRest)
			{
				IWorkTimeStartEndExtractor workTimeStartEndExtractor = new WorkTimeStartEndExtractor();
				IDayOffMaxFlexCalculator dayOffMaxFlexCalculator = new DayOffMaxFlexCalculator(workTimeStartEndExtractor);
				IEnsureWeeklyRestRule ensureWeeklyRestRule = new EnsureWeeklyRestRule(workTimeStartEndExtractor, dayOffMaxFlexCalculator);
				rules.Add(new MinWeeklyRestRule(new WeeksFromScheduleDaysExtractor(), new PersonWeekViolatingWeeklyRestSpecification(new ExtractDayOffFromGivenWeek(), new VerifyWeeklyRestAroundDayOffSpecification(), ensureWeeklyRestRule)));
			}

			_schedulePartModifyAndRollbackService.ClearModificationCollection();
			_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules);
			resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
			resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, false);

			var newRmsValue = calculatePeriodValue(dateOnly, overtimePreferences.SkillActivity, person, timeZoneInfo);
			
			if (!(newRmsValue > oldRmsValue)) return true;

			_schedulePartModifyAndRollbackService.Rollback();
			resourceCalculateDelayer.CalculateIfNeeded(dateOnly, null, false);
			resourceCalculateDelayer.CalculateIfNeeded(dateOnly.AddDays(1), null, false);
			return false;
		}

		private double calculatePeriodValue(DateOnly dateOnly, IActivity activity, IPerson person, TimeZoneInfo timeZoneInfo)
		{
			var aggregateSkill = createAggregateSkill(person, dateOnly, activity);

			if (aggregateSkill != null)
			{
				var aggregatedSkillStaffPeriods = skillStaffPeriods(aggregateSkill, dateOnly.Date, timeZoneInfo);
				double? result = SkillStaffPeriodHelper.SkillDayRootMeanSquare(aggregatedSkillStaffPeriods);
				if (result.HasValue)
					return result.Value;
			}

			return 0;
		}

		private IAggregateSkill createAggregateSkill(IPerson person, DateOnly dateOnly, IActivity activity)
		{
			var skills = aggregateSkills(person, dateOnly).Where(x => x.Activity == activity).ToList();

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

		private static IEnumerable<ISkill> aggregateSkills(IPerson person, DateOnly dateOnly)
		{
			var ret = new List<ISkill>();
			var personPeriod = person.Period(dateOnly);

			foreach (var personSkill in personPeriod.PersonSkillCollection)
			{
				if (!ret.Contains(personSkill.Skill))
					ret.Add(personSkill.Skill);
			}
			return ret;
		}

		private IEnumerable<ISkillStaffPeriod> skillStaffPeriods(IAggregateSkill aggregateSkill, DateTime date, TimeZoneInfo timeZoneInfo)
		{
			_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date, date.AddDays(1), timeZoneInfo));
			return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(date, date.AddDays(1), timeZoneInfo));
		}

		private static bool hasMultiplicatorDefinitionSetOfOvertimeType(IPerson person, DateOnly dateOnly)
		{
			var personPeriod = person.Period(dateOnly);
			if (personPeriod == null) return false;

			var personContract = personPeriod.PersonContract;
			if (personContract == null) return false;

			return personContract.Contract.MultiplicatorDefinitionSetCollection.Any(multiplicatorDefinitionSet => multiplicatorDefinitionSet.MultiplicatorType == MultiplicatorType.Overtime);
		}
	}
}
