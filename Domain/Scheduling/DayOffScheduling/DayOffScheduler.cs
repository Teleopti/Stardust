using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IDayOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions);
	}

	public class DayOffScheduler : IDayOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly ISchedulePartModifyAndRollbackService _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DayOffScheduler(
            IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator, 
			ISchedulePartModifyAndRollbackService schedulePartModifyAndRollbackService, 
            IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
			IHasContractDayOffDefinition hasContractDayOffDefinition)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "3")]
		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
        {
            using (PerformanceOutput.ForOperation("Inital assignment of days off"))
            {
                addDaysOff(matrixList, schedulingOptions);
                addContractDaysOff(matrixListAll, rollbackService, schedulingOptions);
            }
        }

		private void addDaysOff(IEnumerable<IScheduleMatrixPro> matrixList, ISchedulingOptions schedulingOptions)//, IEnumerable<DateOnly> dates, IEnumerable<IPerson> persons)
        {
           
            foreach (var scheduleMatrixPro in matrixList)
            {
                foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
                {
                    var part = scheduleDayPro.DaySchedulePart();
                    if (part.IsScheduled()) continue;

                    var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);

                    if (effectiveRestriction == null || effectiveRestriction.DayOffTemplate == null) continue;
                    // borde inte detta hanteras när effective restriction skapas och då returnera null??
                    if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, effectiveRestriction)) continue;
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

        private void addContractDaysOff(IList<IScheduleMatrixPro> matrixListAll, ISchedulePartModifyAndRollbackService rollbackService, ISchedulingOptions schedulingOptions)
        {
            if (rollbackService == null)
                throw new ArgumentNullException("rollbackService");

            foreach (var matrix in matrixListAll)
            {
                var schedulePeriod = matrix.SchedulePeriod;
                if (!schedulePeriod.IsValid)
                    continue;

                int targetDaysOff;

				IList<IScheduleDay> dayOffDays = new List<IScheduleDay>();
	            var hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out dayOffDays);

				var currentDaysOff = dayOffDays.Count;
                if (hasCorrectNumberOfDaysOff && currentDaysOff > 0)
                    continue;

                foreach (var scheduleDayPro in matrix.UnlockedDays)
                {
                    if (currentDaysOff >= targetDaysOff)
                        continue;

                    IScheduleDay part = scheduleDayPro.DaySchedulePart();
                    if (!_scheduleDayAvailableForDayOffSpecification.IsSatisfiedBy(part))
                        continue;

					if (!_hasContractDayOffDefinition.IsDayOff(part))
                        continue;

					IEffectiveRestriction effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);
					if (effectiveRestriction != null && effectiveRestriction.NotAllowedForDayOffs)
						continue;

					//var existingDayOff = _dayOffsInPeriodCalculator.DayOffInScheduleDayWeek(part, dayOffDays);
					//if (existingDayOff != null)
					//{
					//	dayOffDays.Remove(existingDayOff);
					//	continue;
					//}

                    try
                    {
                        part.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);
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