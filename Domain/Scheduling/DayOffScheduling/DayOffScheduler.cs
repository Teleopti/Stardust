using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Helper;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.ResourceCalculation;
using Teleopti.Ccc.Domain.Scheduling.Assignment;
using Teleopti.Ccc.Domain.Scheduling.Restrictions;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Domain.Scheduling.DayOffScheduling
{
	public interface IDayOffScheduler
	{
		event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;
        void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions, IScheduleTagSetter scheduleTagSetter);
	}

	public class DayOffScheduler : IDayOffScheduler
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;
		private readonly IEffectiveRestrictionCreator _effectiveRestrictionCreator;
		private readonly Func<ISchedulePartModifyAndRollbackService> _schedulePartModifyAndRollbackService;
		private readonly IScheduleDayAvailableForDayOffSpecification _scheduleDayAvailableForDayOffSpecification;
		private readonly IHasContractDayOffDefinition _hasContractDayOffDefinition;

		public event EventHandler<SchedulingServiceBaseEventArgs> DayScheduled;

		public DayOffScheduler(
            IDayOffsInPeriodCalculator dayOffsInPeriodCalculator,
			IEffectiveRestrictionCreator effectiveRestrictionCreator, 
			Func<ISchedulePartModifyAndRollbackService> schedulePartModifyAndRollbackService, 
            IScheduleDayAvailableForDayOffSpecification scheduleDayAvailableForDayOffSpecification,
			IHasContractDayOffDefinition hasContractDayOffDefinition)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
			_effectiveRestrictionCreator = effectiveRestrictionCreator;
			_schedulePartModifyAndRollbackService = schedulePartModifyAndRollbackService;
			_scheduleDayAvailableForDayOffSpecification = scheduleDayAvailableForDayOffSpecification;
			_hasContractDayOffDefinition = hasContractDayOffDefinition;
		}

		public void DayOffScheduling(IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions, IScheduleTagSetter scheduleTagSetter)
        {
            using (PerformanceOutput.ForOperation("Inital assignment of days off"))
            {
                addDaysOff(matrixList, schedulingOptions, scheduleTagSetter);
                addContractDaysOff(matrixList, rollbackService, schedulingOptions);
            }
        }

		private void addDaysOff(IEnumerable<IScheduleMatrixPro> matrixList, SchedulingOptions schedulingOptions, IScheduleTagSetter scheduleTagSetter)
		{
			var cancel = false;
            foreach (var scheduleMatrixPro in matrixList)
            {
                foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
                {
                    var part = scheduleDayPro.DaySchedulePart();
                    if (part.IsScheduled()) continue;

                    var effectiveRestriction = _effectiveRestrictionCreator.GetEffectiveRestriction(part, schedulingOptions);

                    if (effectiveRestriction == null || effectiveRestriction.DayOffTemplate == null) continue;
                    // borde inte detta hanteras när effective restriction skapas och d?returnera null??
                    if (EffectiveRestrictionCreator.OptionsConflictWithRestrictions(schedulingOptions, effectiveRestriction)) continue;
	                var schedulePartModifyAndRollbackService = _schedulePartModifyAndRollbackService();
	                try
                    {
                        part.CreateAndAddDayOff(effectiveRestriction.DayOffTemplate);
                        schedulePartModifyAndRollbackService.Modify(part, scheduleTagSetter);
                    }
                    catch (DayOffOutsideScheduleException)
                    {
                        schedulePartModifyAndRollbackService.Rollback();
                    }

                    var eventArgs = new SchedulingServiceSuccessfulEventArgs(part, () => cancel=true);
                    OnDayScheduled(eventArgs);
                    if (cancel || eventArgs.Cancel) return;
                }       
            }          
        }

        private void addContractDaysOff(IList<IScheduleMatrixPro> matrixList, ISchedulePartModifyAndRollbackService rollbackService, SchedulingOptions schedulingOptions)
        {
            if (rollbackService == null)
                throw new ArgumentNullException(nameof(rollbackService));

	        var cancel = false;
            foreach (var matrix in matrixList)
            {
                var schedulePeriod = matrix.SchedulePeriod;
                if (!schedulePeriod.IsValid)
                    continue;

	            var unlockedDates = matrix.UnlockedDays.Select(sdp => sdp.Day).ToList();

                int targetDaysOff;

				IList<IScheduleDay> dayOffDays;
	            var hasCorrectNumberOfDaysOff = _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out dayOffDays);

				if (hasCorrectNumberOfDaysOff && dayOffDays.Count > 0)
                    continue;

	            var foundSpot = true;

				while (dayOffDays.Count < targetDaysOff && foundSpot)
	            {
					var sortedWeeks = _dayOffsInPeriodCalculator.WeekPeriodsSortedOnDayOff(matrix);
					foundSpot = false;

					foreach (var dayOffOnPeriod in sortedWeeks)
					{
						var bestScheduleDay = dayOffOnPeriod.FindBestSpotForDayOff(_hasContractDayOffDefinition, _scheduleDayAvailableForDayOffSpecification, _effectiveRestrictionCreator, schedulingOptions);
						if (bestScheduleDay == null)
							continue;

						if(!unlockedDates.Contains(bestScheduleDay.DateOnlyAsPeriod.DateOnly))
							continue;

						try
						{
							bestScheduleDay.CreateAndAddDayOff(schedulingOptions.DayOffTemplate);

							var personAssignment = bestScheduleDay.PersonAssignment();
							var authorization = PrincipalAuthorization.Current();
							if (!(authorization.IsPermitted(personAssignment.FunctionPath, bestScheduleDay.DateOnlyAsPeriod.DateOnly, bestScheduleDay.Person))) continue;

							rollbackService.Modify(bestScheduleDay);
							foundSpot = true;
						}
						catch (DayOffOutsideScheduleException)
						{
							rollbackService.Rollback();
						}
						var eventArgs = new SchedulingServiceSuccessfulEventArgs(bestScheduleDay,()=>cancel=true);
						OnDayScheduled(eventArgs);
						if (cancel || eventArgs.Cancel)
							return;

						break;	
					}

					_dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(schedulePeriod, out targetDaysOff, out dayOffDays);
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