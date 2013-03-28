using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock
{
	public interface ITeamBlockRestrictionOverLimitValidator
	{
		bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences);
	}

	public class TeamBlockRestrictionOverLimitValidator : ITeamBlockRestrictionOverLimitValidator
	{
		private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider;
		private readonly IDictionary<IPerson, IScheduleRange> _allSelectedScheduleRangeClones;
		private readonly IScheduleDayEquator _scheduleDayEquator;

		public TeamBlockRestrictionOverLimitValidator(IRestrictionOverLimitDecider restrictionOverLimitDecider, IDictionary<IPerson, IScheduleRange> allSelectedScheduleRangeClones, IScheduleDayEquator scheduleDayEquator)
		{
			_restrictionOverLimitDecider = restrictionOverLimitDecider;
			_allSelectedScheduleRangeClones = allSelectedScheduleRangeClones;
			_scheduleDayEquator = scheduleDayEquator;
		}

		public bool Validate(ITeamBlockInfo teamBlockInfo, IOptimizationPreferences optimizationPreferences)
		{
			foreach (var matrix in teamBlockInfo.MatrixesForGroupAndBlock())
			{
				if (!validateMatrixMinMaxDays(matrix, optimizationPreferences, _allSelectedScheduleRangeClones, _scheduleDayEquator))
					return false;

				if (preferencesOverLimit(matrix, optimizationPreferences))
					return false;

				if (mustHavesOverLimit(matrix, optimizationPreferences))
					return false;

				if (rotationOverLimit(matrix, optimizationPreferences))
					return false;

				if (availabilitiesOverLimit(matrix, optimizationPreferences))
					return false;

				if (studentAvailabilitiesOverLimit(matrix, optimizationPreferences))
					return false;
			}

			return true;
		}

		// class
		private bool validateMatrixMinMaxDays(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences, IDictionary<IPerson, IScheduleRange> allSelectedScheduleRangeClones, IScheduleDayEquator scheduleDayEquator)
		{
			int originalNumberOfDaysOff = 0;
			int originalNumberOfWorkShifts = 0;
			int changedDaysOff = 0;
			int changedShifts = 0;

			if (!optimizationPreferences.Shifts.KeepShifts && !optimizationPreferences.DaysOff.UseKeepExistingDaysOff)
				return true;

			IScheduleRange rangeCloneForMatrix = allSelectedScheduleRangeClones[matrix.Person];
			foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
			{
				IScheduleDay currentScheduleDay = scheduleDayPro.DaySchedulePart();
				IScheduleDay originalScheduleDay = rangeCloneForMatrix.ScheduledDay(scheduleDayPro.Day);

				var originalSignificantPart = originalScheduleDay.SignificantPart();
				if (originalSignificantPart == SchedulePartView.DayOff)
					originalNumberOfDaysOff++;

				if (originalSignificantPart == SchedulePartView.MainShift)
					originalNumberOfWorkShifts++;

				if (!scheduleDayEquator.DayOffEquals(originalScheduleDay, currentScheduleDay))
                    changedDaysOff++;

				if (!scheduleDayEquator.MainShiftEquals(originalScheduleDay, currentScheduleDay))
					changedShifts++;

			}
			if (optimizationPreferences.Shifts.KeepShifts && optimizationPreferences.Shifts.KeepShiftsValue > 1 - ((double)changedShifts / originalNumberOfWorkShifts))
				return false;

			if (optimizationPreferences.DaysOff.UseKeepExistingDaysOff && optimizationPreferences.DaysOff.KeepExistingDaysOffValue > 1 - ((double)changedDaysOff/originalNumberOfDaysOff))
				return false;

			return true;
		}

		private bool preferencesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UsePreferences)
				return false;
			return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(optimizationPreferences.General.PreferencesValue), matrix).BrokenDays.Any();
		}

		private bool mustHavesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseMustHaves)
				return false;
			return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(optimizationPreferences.General.MustHavesValue), matrix).BrokenDays.Any();
		}

		private bool rotationOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseRotations)
				return false;
			return _restrictionOverLimitDecider.RotationOverLimit(new Percent(optimizationPreferences.General.RotationsValue), matrix).BrokenDays.Any();
		}

		private bool availabilitiesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseAvailabilities)
				return false;
			return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(optimizationPreferences.General.AvailabilitiesValue), matrix).BrokenDays.Any();
		}

		private bool studentAvailabilitiesOverLimit(IScheduleMatrixPro matrix, IOptimizationPreferences optimizationPreferences)
		{
			if (!optimizationPreferences.General.UseStudentAvailabilities)
				return false;
			return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(optimizationPreferences.General.StudentAvailabilitiesValue), matrix).BrokenDays.Any();
		}
	}
}