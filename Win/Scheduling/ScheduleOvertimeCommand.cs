using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.ScheduleTagging;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
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
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private readonly IProjectionProvider _projectionProvider;
		private BackgroundWorker _backgroundWorker;

		public ScheduleOvertimeCommand(ISchedulingResultStateHolder schedulingResultStateHolder,
		                               IOvertimeLengthDecider overtimeLengthDecider,
		                               ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               ISchedulerStateHolder schedulerStateHolder,
		                               IProjectionProvider projectionProvider)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_schedulerStateHolder = schedulerStateHolder;
			_projectionProvider = projectionProvider;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

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
					if (overtimePreferences.ScheduleTag.Description != "<None>")
					{
						IScheduleTagSetter scheduleTagSetter = new ScheduleTagSetter(overtimePreferences.ScheduleTag);
						_schedulePartModifyAndRollbackService.Modify(scheduleDay, scheduleTagSetter, NewBusinessRuleCollection.AllForScheduling(
																	 _schedulerStateHolder.SchedulingResultState));
					}
					else
					_schedulePartModifyAndRollbackService.Modify(scheduleDay, NewBusinessRuleCollection.AllForScheduling(
																	 _schedulerStateHolder.SchedulingResultState));
					OnDayScheduled(new SchedulingServiceBaseEventArgs(scheduleDay));
					resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
																			  overtimeLayerPeriod,
																new List<IScheduleDay> { scheduleDay });
				}
			}
		}

		protected virtual void OnDayScheduled(SchedulingServiceBaseEventArgs scheduleServiceBaseEventArgs)
		{
			EventHandler<SchedulingServiceBaseEventArgs> temp = DayScheduled;
			if (temp != null)
			{
				temp(this, scheduleServiceBaseEventArgs);
			}
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
