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
            _optimizationPreferences = optimizationPreferences;
            _originalStateContainer = originalStateContainer;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(matrix, restrictionChecker);
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
            if (!_optimizationPreferences.Extra.KeepShifts)
                return false;

            int changedDaysOff = _originalStateContainer.CountChangedDayOffs();
            int changedWorkShifts = _originalStateContainer.CountChangedWorkShifts();
            int numberOfDays = _originalStateContainer.ScheduleMatrix.EffectivePeriodDays.Count;
            int movedDays = changedDaysOff + changedWorkShifts;

            if (numberOfDays == 0)
                return false;

            double movedPercent = (double)movedDays / numberOfDays;
            double keepPercent = 1 - movedPercent;

            return keepPercent < _optimizationPreferences.Extra.KeepShiftsValue;
        }

        private IList<DateOnly> preferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(_optimizationPreferences.General.PreferencesValue)).BrokenDays;
        }

        private IList<DateOnly> mustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(_optimizationPreferences.General.MustHavesValue)).BrokenDays;
        }

        private IList<DateOnly> rotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.RotationOverLimit(new Percent(_optimizationPreferences.General.RotationsValue)).BrokenDays;
        }

        private IList<DateOnly> availabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.AvailabilitiesValue)).BrokenDays;
        }

        private IList<DateOnly> studentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.StudentAvailabilitiesValue)).BrokenDays;
        }
    }
}
