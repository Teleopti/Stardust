using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.WinCode.Scheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public interface IScheduleOvertimeCommand
	{
		void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
												IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager);
	}

	public class ScheduleOvertimeCommand : IScheduleOvertimeCommand
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IProjectionProvider _projectionProvider;
		private BackgroundWorker _backgroundWorker;

		public ScheduleOvertimeCommand(ISchedulerStateHolder schedulerState,
		                               ISchedulingResultStateHolder schedulingResultStateHolder,
		                               IOvertimeLengthDecider overtimeLengthDecider,
		                               ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IProjectionProvider projectionProvider)
		{
			_schedulerState = schedulerState;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_projectionProvider = projectionProvider;
		}

		public void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
							IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer, IGridlockManager gridlockManager)
		{
			_backgroundWorker = backgroundWorker;
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			foreach (var dateOnly in selectedDates)
			{
				var oldRmsValue = calculatePeriodValue(dateOnly);
				//Randomly select one of the selected agents that does not end his shift with overtime
				var persons = orderCandidatesRandomly(selectedPersons, dateOnly);
				foreach (var person in persons)
				{
					if (checkIfCancelPressed())
						return;

					var locks = gridlockManager.Gridlocks(person, dateOnly);
					if (locks != null && locks.Count != 0) continue;

					var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
					var scheduleEndTime = _projectionProvider.Projection(scheduleDay).Period().GetValueOrDefault().EndDateTime;
					//Calculate best length (if any) for overtime
					var overtimeLayerLength = _overtimeLengthDecider.Decide(person, dateOnly, scheduleEndTime,
					                                                        overtimePreferences.SkillActivity,
					                                                        new MinMax<TimeSpan>(
						                                                        overtimePreferences.SelectedTimePeriod.StartTime,
						                                                        overtimePreferences.SelectedTimePeriod.EndTime));
					if (overtimeLayerLength == TimeSpan.Zero)
						continue;

					//extend shift
					var overtimeLayerPeriod = new DateTimePeriod(scheduleEndTime, scheduleEndTime.Add(overtimeLayerLength));
					scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity,
					                                 overtimeLayerPeriod,
					                                 overtimePreferences.OvertimeType);
					var rules = NewBusinessRuleCollection.Minimum();
					if (!overtimePreferences.AllowBreakNightlyRest)
						rules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
					if (!overtimePreferences.AllowBreakMaxWorkPerWeek)
						rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
					if (!overtimePreferences.AllowBreakWeeklyRest)
						rules.Add(new MinWeeklyRestRule(
							          new WeeksFromScheduleDaysExtractor(), new WorkTimeStartEndExtractor()));

					_schedulePartModifyAndRollbackService.ClearModificationCollection();

					IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
					_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules);

					resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly, null);
					var newRmsValue = calculatePeriodValue(dateOnly);
					if (newRmsValue > oldRmsValue)
					{
						_schedulePartModifyAndRollbackService.Rollback();
						resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly, null);
					}
					else
					{
						oldRmsValue = newRmsValue;
					}

					OnDayScheduled(new SchedulingServiceSuccessfulEventArgs(scheduleDay));
				}
			}
		}

		private double calculatePeriodValue(DateOnly dateOnly)
		{
			var aggregateSkill = createAggregateSkill();
			var aggregatedSkillStaffPeriods = skillStaffPeriods(aggregateSkill, dateOnly);
			double? result = SkillStaffPeriodHelper.SkillDayRootMeanSquare(aggregatedSkillStaffPeriods);
			if (result.HasValue)
				return result.Value;

			return 0;
		}

		private IAggregateSkill createAggregateSkill()
		{
			var skills = _schedulingResultStateHolder.NonVirtualSkills;
			var aggregateSkillSkill = new Skill("", "", Color.Pink, 15, skills[0].SkillType);
			aggregateSkillSkill.ClearAggregateSkill();
			foreach (ISkill skill in skills)
			{
				aggregateSkillSkill.AddAggregateSkill(skill);
			}
			aggregateSkillSkill.IsVirtual = true;

			if (aggregateSkillSkill.AggregateSkills.Any())
			{
				((ISkill)aggregateSkillSkill).DefaultResolution =
					aggregateSkillSkill.AggregateSkills.Min(s => s.DefaultResolution);
			}

			return aggregateSkillSkill;
		}

		private IEnumerable<ISkillStaffPeriod> skillStaffPeriods(IAggregateSkill aggregateSkill, DateOnly dateOnly)
		{
			//Need to do this twice, don't know why. But now the same code is used by the result view as well. Refactor later
			//Think the problem is when only one skill is open
			_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly, dateOnly.AddDays(1), _schedulerState.TimeZoneInfo));
			return _schedulingResultStateHolder.SkillStaffPeriodHolder.SkillStaffPeriodList(aggregateSkill, TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly, dateOnly.AddDays(1), _schedulerState.TimeZoneInfo));
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs e)
		{
			if (_backgroundWorker.CancellationPending)
			{
				e.Cancel = true;
			}
			_backgroundWorker.ReportProgress(1, e.SchedulePart);
		}

		private IEnumerable<IPerson> orderCandidatesRandomly(IEnumerable<IPerson> persons, DateOnly dateOnly)
		{
			var personsHaveNoOvertime = new List<IPerson>();
			var randomizedPersons = persons.Randomize();
			foreach (var person in randomizedPersons)
			{
				var schedule = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
				if (schedule.SignificantPart() != SchedulePartView.MainShift) continue;
				var projection = _projectionProvider.Projection(schedule);
				var lastLayer = projection.Last();
				if (lastLayer.DefinitionSet != null && lastLayer.DefinitionSet.MultiplicatorType == MultiplicatorType.Overtime)
					continue;
				if (((VisualLayer)lastLayer).HighestPriorityAbsence != null)
					continue;
				personsHaveNoOvertime.Add(person);
			}
			return personsHaveNoOvertime;
		}


        private bool checkIfCancelPressed()
        {
            if (_backgroundWorker.CancellationPending)
              return  true;
            return false;
        }
	}
}
