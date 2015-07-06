

using System.Collections.Generic;
using Teleopti.Ccc.Domain.Optimization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IMaxMovedDaysOverLimitValidator
	{
		bool ValidateMatrix(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences);
	}

	public class MaxMovedDaysOverLimitValidator : IMaxMovedDaysOverLimitValidator
	{
		private readonly IDictionary<IPerson, IScheduleRange> _allSelectedScheduleRangeClones;
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public MaxMovedDaysOverLimitValidator(IDictionary<IPerson, IScheduleRange> allSelectedScheduleRangeClones, IScheduleDayEquator scheduleDayEquator)
		{
			_allSelectedScheduleRangeClones = allSelectedScheduleRangeClones;
			_scheduleDayEquator = scheduleDayEquator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ValidateMatrix(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			int originalNumberOfDaysOff = 0;
			int originalNumberOfWorkShifts = 0;
			int changedDaysOff = 0;
			int changedShifts = 0;

			if (!optimizationPreferences.Shifts.KeepShifts && !optimizationPreferences.DaysOff.UseKeepExistingDaysOff)
				return true;

			//IScheduleRange rangeCloneForMatrix = null;
			IScheduleRange rangeCloneForMatrix;
			if(!_allSelectedScheduleRangeClones.TryGetValue(matrix.Person,out rangeCloneForMatrix))
				return false;
			foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
			{
				IScheduleDay currentScheduleDay = scheduleDayPro.DaySchedulePart();
				IScheduleDay originalScheduleDay = rangeCloneForMatrix.ScheduledDay(scheduleDayPro.Day);

				var originalSignificantPart = originalScheduleDay.SignificantPart();
				if (optimizationPreferences.DaysOff.UseKeepExistingDaysOff && originalSignificantPart == SchedulePartView.DayOff)
					originalNumberOfDaysOff++;

				if (optimizationPreferences.Shifts.KeepShifts && originalSignificantPart == SchedulePartView.MainShift)
					originalNumberOfWorkShifts++;

				if (optimizationPreferences.DaysOff.UseKeepExistingDaysOff && !_scheduleDayEquator.DayOffEquals(originalScheduleDay, currentScheduleDay))
					changedDaysOff++;

				if (optimizationPreferences.Shifts.KeepShifts && !_scheduleDayEquator.MainShiftEquals(originalScheduleDay, currentScheduleDay))
					changedShifts++;

			}
			if (optimizationPreferences.Shifts.KeepShifts && optimizationPreferences.Shifts.KeepShiftsValue > 1 - ((double)changedShifts / originalNumberOfWorkShifts))
				return false;

			if (optimizationPreferences.DaysOff.UseKeepExistingDaysOff && optimizationPreferences.DaysOff.KeepExistingDaysOffValue > 1 - ((double)changedDaysOff / originalNumberOfDaysOff))
				return false;

			return true;
		} 
	}
}