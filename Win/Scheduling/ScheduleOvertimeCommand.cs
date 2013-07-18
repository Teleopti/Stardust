using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Overtime;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Ccc.WinCode.Common;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Win.Scheduling
{
	public interface IScheduleOvertimeCommand
	{
		void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
												IList<IScheduleDay> selectedSchedules);
	}

	public class ScheduleOvertimeCommand : IScheduleOvertimeCommand
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IOvertimeLengthDecider _overtimeLengthDecider;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IResourceCalculateDelayer _resourceCalculateDelayer;
		private readonly ISchedulerStateHolder _schedulerStateHolder;
		private BackgroundWorker _backgroundWorker;

		public ScheduleOvertimeCommand(ISchedulingResultStateHolder schedulingResultStateHolder,
		                               IOvertimeLengthDecider overtimeLengthDecider,
		                               ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService,
		                               IResourceCalculateDelayer resourceCalculateDelayer,
		                               ISchedulerStateHolder schedulerStateHolder)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_overtimeLengthDecider = overtimeLengthDecider;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_resourceCalculateDelayer = resourceCalculateDelayer;
			_schedulerStateHolder = schedulerStateHolder;
		}

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public void Exectue(IOvertimePreferences overtimePreferences, BackgroundWorker backgroundWorker,
							 IList<IScheduleDay> selectedSchedules)
		{
			_backgroundWorker = backgroundWorker;
			var selectedDates = selectedSchedules.Select(x => x.DateOnlyAsPeriod.DateOnly).Distinct();
			var selectedPersons = selectedSchedules.Select(x => x.Person).Distinct().ToList();
			foreach (var dateOnly in selectedDates)
			{
				//Randomly select one of the selected agents that does not end his shift with overtime
				var person = selectPersonRandomly(selectedPersons, dateOnly);
				var scheduleDay = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
				
				//Calculate best length (if any) for overtime
				var overtimeLayerLength = _overtimeLengthDecider.Decide(person, dateOnly, scheduleDay.Period.EndDateTime,
				                                                        overtimePreferences.SkillActivity,
				                                                        new MinMax<TimeSpan>(
					                                                        overtimePreferences.SelectedTimePeriod.StartTime,
					                                                        overtimePreferences.SelectedTimePeriod.EndTime));
				if(overtimeLayerLength ==TimeSpan.Zero)
					continue;

				//extend shift
				var scheduleEndTime = scheduleDay.PersonAssignment().ProjectionService().CreateProjection().Period().GetValueOrDefault().EndDateTime;
				var overtimeLayerPeriod = new DateTimePeriod(scheduleEndTime, scheduleEndTime.Add(overtimeLayerLength));
				 scheduleDay.CreateAndAddOvertime(overtimePreferences.SkillActivity,
				                              overtimeLayerPeriod,
				                              overtimePreferences.OvertimeType);
				_schedulePartModifyAndRollbackService.Modify(scheduleDay,
				                                             NewBusinessRuleCollection.AllForScheduling(
					                                             _schedulerStateHolder.SchedulingResultState));
				OnDayScheduled(new SchedulingServiceBaseEventArgs(scheduleDay));
				_resourceCalculateDelayer.CalculateIfNeeded(scheduleDay.DateOnlyAsPeriod.DateOnly,
																		  overtimeLayerPeriod,
															new List<IScheduleDay> { scheduleDay });
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

		private IPerson selectPersonRandomly(IList<IPerson> persons, DateOnly dateOnly)
		{
			var personsHaveNoOvertime = new List<IPerson>();
			foreach (var person in persons)
			{
				var schedule = _schedulingResultStateHolder.Schedules[person].ScheduledDay(dateOnly);
				if (schedule.PersonAssignment().ProjectionService().CreateProjection().Overtime() > TimeSpan.Zero)
					continue;
				personsHaveNoOvertime.Add(person);
			}
			if (personsHaveNoOvertime.Count > 0)
				return personsHaveNoOvertime.GetRandom();
			return null;
		}
	}
}
