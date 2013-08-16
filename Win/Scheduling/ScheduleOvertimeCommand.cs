using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Obfuscated.ResourceCalculation;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public interface IScheduleOvertimeCommand
	{
		void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
                                                IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer);
	}

	public class ScheduleOvertimeCommand : IScheduleOvertimeCommand
	{
		private readonly ISchedulerStateHolder _schedulerState;
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IProjectionProvider _projectionProvider;
		private readonly ISkillResolutionProvider _resolutionProvider;
		private BackgroundWorker _backgroundWorker;

		public ScheduleOvertimeCommand(ISchedulerStateHolder schedulerState,
		                               ISchedulingResultStateHolder schedulingResultStateHolder,
		                               IOvertimeLengthDecider overtimeLengthDecider,
		                               ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IProjectionProvider projectionProvider,
		                               ISkillResolutionProvider resolutionProvider)
		{
			_schedulerState = schedulerState;
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_projectionProvider = projectionProvider;
			_resolutionProvider = resolutionProvider;
		}

		public void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
                             IList<IScheduleDay> selectedSchedules, IResourceCalculateDelayer resourceCalculateDelayer)
		{
			_backgroundWorker = backgroundWorker;
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			foreach (var dateOnly in selectedDates)
			{
				var persons = orderCandidatesRandomly(selectedPersons, dateOnly);
				foreach (var person in persons)
				{
					if (checkIfCancelPressed()) return;
					//Randomly select one of the selected agents that does not end his shift with overtime
					var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
					if (scheduleDay.SignificantPart() != SchedulePartView.MainShift) continue;
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
					if(!overtimePreferences.AllowBreakNightlyRest)
						rules.Add(new NewNightlyRestRule(new WorkTimeStartEndExtractor()));
					if(!overtimePreferences.AllowBreakMaxWorkPerWeek)
						rules.Add(new NewMaxWeekWorkTimeRule(new WeeksFromScheduleDaysExtractor()));
					if(!overtimePreferences.AllowBreakWeeklyRest)
						rules.Add(new MinWeeklyRestRule(
							          new WeeksFromScheduleDaysExtractor(), new WorkTimeStartEndExtractor()));
                    
                    _schedulePartModifyAndRollbackService.ClearModificationCollection();
				    var oldRmsValue = calculatePeriodValue(dateOnly);

					if (overtimePreferences.ScheduleTag.Description != "<None>")
					{
						IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
						_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, scheduleTagSetter, rules);
					}
					else
						_schedulePartModifyAndRollbackService.ModifyStrictly(scheduleDay, rules);

                    
					OnDayScheduled(new SchedulingServiceBaseEventArgs(scheduleDay));
					resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
																			  overtimeLayerPeriod,
																new List<IScheduleDay> { scheduleDay });
				    var newRmsValue = calculatePeriodValue(dateOnly);
				    if (newRmsValue > oldRmsValue)
				        _schedulePartModifyAndRollbackService.Rollback();
				}
			}
		}

        private double? calculatePeriodValue(DateOnly dateOnly)
        {
            var skills = _schedulingResultStateHolder.NonVirtualSkills;
			var minimumResolution = _resolutionProvider.MinimumResolution(skills);
			// in ViewPoint
			var timeZone = _schedulerState.TimeZoneInfo;
			// user timeZone
			var userTimeZone = TeleoptiPrincipal.Current.Regional.TimeZone;
			var diff = userTimeZone.BaseUtcOffset - timeZone.BaseUtcOffset;
			var viewPointPeriod = new DateOnlyPeriod(dateOnly, dateOnly).ToDateTimePeriod(timeZone).MovePeriod(diff);    
			var dateTimePeriods = viewPointPeriod.WholeDayCollection();

			IDictionary<DateTimePeriod, IList<ISkillStaffPeriod>> skillStaffPeriods = new Dictionary<DateTimePeriod, IList<ISkillStaffPeriod>>();

			foreach (DateTimePeriod utcPeriod in dateTimePeriods)
			{
				DateTimePeriod period = utcPeriod;
				foreach (ISkill skill in skills)
				{
					ISkillStaffPeriodDictionary content;
					if (!_schedulingResultStateHolder.SkillStaffPeriodHolder.SkillSkillStaffPeriodDictionary.TryGetValue(skill, out content)) continue;

					if (skill.DefaultResolution > minimumResolution)
					{
						
						var relevantSkillStaffPeriodList = content.Where(c => period.Contains(c.Key)).Select(c => c.Value);
						var skillStaffPeriodsSplitList = new List<ISkillStaffPeriod>();
						double factor = (double)minimumResolution / skill.DefaultResolution;
						foreach (ISkillStaffPeriod skillStaffPeriod in relevantSkillStaffPeriodList)
						{
							skillStaffPeriodsSplitList.AddRange(SkillStaffPeriodHolder.SplitSkillStaffPeriod(skillStaffPeriod, factor, TimeSpan.FromMinutes(minimumResolution)));
						}

						foreach (ISkillStaffPeriod skillStaffPeriod in skillStaffPeriodsSplitList)
						{
							IList<ISkillStaffPeriod> foundList;
							if (!skillStaffPeriods.TryGetValue(skillStaffPeriod.Period, out foundList))
							{
								foundList = new List<ISkillStaffPeriod>();
								skillStaffPeriods.Add(skillStaffPeriod.Period, foundList);
							}

							foundList.Add(skillStaffPeriod);
						}
					}
					else
					{
						foreach (KeyValuePair<DateTimePeriod, ISkillStaffPeriod> pair in content.Where(c => period.Contains(c.Key)))
						{
							IList<ISkillStaffPeriod> foundList;
							if (!skillStaffPeriods.TryGetValue(pair.Key, out foundList))
							{
								foundList = new List<ISkillStaffPeriod>();
								skillStaffPeriods.Add(pair.Key, foundList);
							}

							foundList.Add(pair.Value);
						}
					}
				}

			}
			IList<ISkillStaffPeriod> combinedSkillStaffPeriods = new List<ISkillStaffPeriod>();

			foreach (KeyValuePair<DateTimePeriod, IList<ISkillStaffPeriod>> keyValuePair in skillStaffPeriods)
			{
				ISkillStaffPeriod tempPeriod = SkillStaffPeriod.Combine(keyValuePair.Value);
				var aggregate = (IAggregateSkillStaffPeriod)tempPeriod;
				aggregate.IsAggregate = true;

				foreach (ISkillStaffPeriod staffPeriod in keyValuePair.Value)
				{
					var asAgg = (IAggregateSkillStaffPeriod)staffPeriod;
					if (!asAgg.IsAggregate)
					{
						SkillStaffPeriodHolder.HandleAggregate(keyValuePair, aggregate, staffPeriod, asAgg);
					}
					else
					{
						aggregate.CombineAggregatedSkillStaffPeriod(asAgg);
					}

				}
				combinedSkillStaffPeriods.Add(tempPeriod);
			}

			return SkillStaffPeriodHelper.SkillDayRootMeanSquare(combinedSkillStaffPeriods);
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
