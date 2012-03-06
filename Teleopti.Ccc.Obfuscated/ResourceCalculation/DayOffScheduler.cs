using System;
using System.Collections;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
{
	public interface IDayOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void DayOffScheduling(IEnumerable<IScheduleDay> selectedParts, IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class DayOffScheduler : IDayOffScheduler
	{
		private readonly ISchedulingResultStateHolder _schedulingResultStateHolder;
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
	    private readonly IScheduleMatrixListCreator _scheduleMatrixListCreator;

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DayOffScheduler(
            ISchedulingResultStateHolder schedulingResultStateHolder, 
            IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator, 
            ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
            IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
            IScheduleMatrixListCreator scheduleMatrixListCreator)
		{
			_schedulingResultStateHolder = schedulingResultStateHolder;
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingOptions = schedulingOptions;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
		    _scheduleMatrixListCreator = scheduleMatrixListCreator;
		}

		public void DayOffScheduling(IEnumerable<IScheduleDay> selectedParts, IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons, ISchedulePartModifyAndRollbackService rollbackService)
		{
			using (PerformanceOutput.ForOperation("Inital assignment of days off"))
			{
				addDaysOff(dates, persons);
				if (_schedulingOptions.AddContractScheduleDaysOff)
                    addContractDaysOff(selectedParts, rollbackService);
			}
		}

		private void addDaysOff(IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons)
		{
			foreach (DateOnly date in dates)
			{
				foreach (IPerson person in persons)
				{
					IScheduleDay part = _schedulingResultStateHolder.Schedules[person].ScheduledDay(date);
					if (part.IsScheduled())
						continue;

					IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, _schedulingOptions);

					if (effectiveRestriction != null && effectiveRestriction.DayOffTemplate != null)
					{
						// borde inte detta hanteras när effective restriction skapas och då returnera null??
						if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_schedulingOptions, effectiveRestriction))
							continue;
						try
						{
							part.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate);
							_schedulePartModifyAndRollbackService.Modify(part);
						}
						catch (DayOffOutsideScheduleException)
						{
							_schedulePartModifyAndRollbackService.Rollback();
						}

						var eventArgs = new SchedulingServiceBaseEventArgs(part);
						OnDayScheduled(eventArgs);
						if (eventArgs.Cancel) return;
					}
				}
			}
		}

		private void addContractDaysOff(IEnumerable<IScheduleDay> selectedParts, ISchedulePartModifyAndRollbackService rollbackService)
		{
            if (rollbackService == null)
				throw new ArgumentNullException("rollbackService");

		    IList<IScheduleMatrixPro> matrixlist = _scheduleMatrixListCreator.CreateMatrixListFromScheduleParts(selectedParts);
		    foreach (var matrix in matrixlist)
		    {
		        var schedulePeriod = matrix.SchedulePeriod;
		        if (!schedulePeriod.IsValid)
		            continue;

		        EmploymentType employmentType = schedulePeriod.Contract.EmploymentType;
		        if (employmentType == EmploymentType.HourlyStaff)
		            continue;

		        int targetDaysOff;
		        int currentDaysOff;
		        bool hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod,
		                                                                                              out targetDaysOff,
		                                                                                              out currentDaysOff);
		        if (hasCorrectNumberOfDaysOff && currentDaysOff > 0)
		            continue;

                //if (employmentType == EmploymentType.FixedStaffDayWorkTime || employmentType == EmploymentType.FixedStaffPeriodWorkTime)
                //{
                //    if (currentDaysOff != 0)
                //        continue;
                //}

                matrix.UnlockPeriod(schedulePeriod.DateOnlyPeriod);

		        foreach (var scheduleDayPro in matrix.UnlockedDays)
		        {
		            if (currentDaysOff >= targetDaysOff)
                        continue;

		            IScheduleDay part = scheduleDayPro.DaySchedulePart();
                    if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
                        continue;

                    if (schedulePeriod.ContractSchedule.IsWorkday(schedulePeriod.DateOnlyPeriod.StartDate, scheduleDayPro.Day))
		                continue;

                    try
                    {
                        part.CreateAndAddDayOff(_schedulingOptions.DayOffTemplate);
                        rollbackService.Modify(part);
                        currentDaysOff++;
                    }
                    catch (DayOffOutsideScheduleException)
                    {
                        rollbackService.Rollback();
                    }
                    var eventArgs = new SchedulingServiceBaseEventArgs(part);
                    OnDayScheduled(eventArgs);
                    if (eventArgs.Cancel) 
                        return;
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
	}
}