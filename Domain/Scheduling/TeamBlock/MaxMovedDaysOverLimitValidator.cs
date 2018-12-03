using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Optimization;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface IMaxMovedDaysOverLimitValidator
	{
		bool ValidateMatrix(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences, IDaysOffPreferences daysOffPreferences);
	}

	public class MaxMovedDaysOverLimitValidator : IMaxMovedDaysOverLimitValidator
	{
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public MaxMovedDaysOverLimitValidator(IScheduleDayEquator scheduleDayEquator)
		{
			_scheduleDayEquator = scheduleDayEquator;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1")]
		public bool ValidateMatrix(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences, IDaysOffPreferences daysOffPreferences)
		{
			int originalNumberOfDaysOff = 0;
			int changedDaysOff = 0;

			if (!daysOffPreferences.UseKeepExistingDaysOff)
				return true;

			var allSelectedScheduleRangeClones = optimizationPreferences.Rescheduling.AllSelectedScheduleRangeClones;
			InParameter.ListCannotBeEmpty(nameof(optimizationPreferences.Rescheduling.AllSelectedScheduleRangeClones.Keys), optimizationPreferences.Rescheduling.AllSelectedScheduleRangeClones.Keys);
				
			IScheduleRange rangeCloneForMatrix = allSelectedScheduleRangeClones[matrix.Person];
			foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
			{
				IScheduleDay currentScheduleDay = scheduleDayPro.DaySchedulePart();
				IScheduleDay originalScheduleDay = rangeCloneForMatrix.ScheduledDay(scheduleDayPro.Day);

				var originalSignificantPart = originalScheduleDay.SignificantPart();
				if (daysOffPreferences.UseKeepExistingDaysOff && originalSignificantPart == SchedulePartView.DayOff)
					originalNumberOfDaysOff++;

				if (daysOffPreferences.UseKeepExistingDaysOff && !_scheduleDayEquator.DayOffEquals(originalScheduleDay, currentScheduleDay))
					changedDaysOff++;
			}
			if (daysOffPreferences.UseKeepExistingDaysOff && daysOffPreferences.KeepExistingDaysOffValue > 1 - ((double)changedDaysOff / originalNumberOfDaysOff))
				return false;

			return true;
		} 
	}
}