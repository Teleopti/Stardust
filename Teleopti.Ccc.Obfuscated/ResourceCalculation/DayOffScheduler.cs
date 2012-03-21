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
        void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService);
	}

	public class DayOffScheduler : IDayOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulingOptions _schedulingOptions;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;

	    public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DayOffScheduler(ISchedulingResultStateHolder schedulingResultStateHolder, IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator, ISchedulingOptions schedulingOptions,
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
            IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
            IScheduleMatrixListCreator scheduleMatrixListCreator)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulingOptions = schedulingOptions;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
		}

        public void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService)
        {
            using (PerformanceOutput.ForOperation("Inital assignment of days off"))
            {
                addDaysOff(matrixList);
                if (_schedulingOptions.AddContractScheduleDaysOff) addContractDaysOff(matrixListAll, rollbackService);
            }
        }

        private void addDaysOff(IEnumerable<IScheduleMatrixPro> matrixList)//, IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons)
        {
           
            foreach (var scheduleMatrixPro in matrixList)
            {
                foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
                {
                    var part = scheduleDayPro.DaySchedulePart();
                    if (part.IsScheduled()) continue;

                    var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, _schedulingOptions);

                    if (effectiveRestriction == null || effectiveRestriction.DayOffTemplate == null) continue;
                    // borde inte detta hanteras när effective restriction skapas och då returnera null??
                    if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(_schedulingOptions, effectiveRestriction)) continue;
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

        private void addContractDaysOff(IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService)
        {
            if (rollbackService == null)
                throw new ArgumentNullException("rollbackService");

            foreach (var matrix in matrixListAll)
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

                foreach (var scheduleDayPro in matrix.UnlockedDays)
                {
                    if (currentDaysOff >= targetDaysOff)
                        continue;

                    IScheduleDay part = scheduleDayPro.DaySchedulePart();
                    if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
                        continue;

                    if (schedulePeriod.ContractSchedule.IsWorkday(schedulePeriod.DateOnlyPeriod.StartDate, scheduleDayPro.Day))
                        continue;
                    IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, _schedulingOptions);

                    if (effectiveRestriction != null && effectiveRestriction.IsLimitedWorkday)
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