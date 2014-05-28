using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IOptimizationOverLimitByRestrictionDecider
    {
        IList<DateOnly> OverLimit();
        bool MoveMaxDaysOverLimit();
    }

    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitByRestrictionDecider
    {
	    private readonly IScheduleMatrixPro _matrix;
	    private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IScheduleMatrixOriginalStateContainer _originalStateContainer;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixPro matrix,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences,
            IScheduleMatrixOriginalStateContainer originalStateContainer
            )
        {
	        _matrix = matrix;
	        _optimizationPreferences = optimizationPreferences;
            _originalStateContainer = originalStateContainer;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(restrictionChecker);
        }

        public IList<DateOnly> OverLimit()
        {
            List<DateOnly> overallResult = new List<DateOnly>();
            overallResult.AddRange(preferencesOverLimit());
            overallResult.AddRange(mustHavesOverLimit());
            overallResult.AddRange(rotationOverLimit());
            overallResult.AddRange(availabilitiesOverLimit());
            overallResult.AddRange(studentAvailabilitiesOverLimit());

            return overallResult;
        }

        public bool MoveMaxDaysOverLimit()
        {
            if (_optimizationPreferences.Shifts.KeepShifts && _optimizationPreferences.Shifts.KeepShiftsValue > 1 - _originalStateContainer.ChangedWorkShiftsPercent())
                return true;

			if (_optimizationPreferences.DaysOff.UseKeepExistingDaysOff && _optimizationPreferences.DaysOff.KeepExistingDaysOffValue > 1 - _originalStateContainer.ChangedDayOffsPercent())
                return true;

            return false;
        }

        private IList<DateOnly> preferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(_optimizationPreferences.General.PreferencesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> mustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(_optimizationPreferences.General.MustHavesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> rotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.RotationOverLimit(new Percent(_optimizationPreferences.General.RotationsValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> availabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.AvailabilitiesValue), _matrix).BrokenDays;
        }

        private IList<DateOnly> studentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return new List<DateOnly>();
			return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.StudentAvailabilitiesValue), _matrix).BrokenDays;
        }
    }
}
