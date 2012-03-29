using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public interface IOptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitDecider, IRestrictionsOverLimitDecider{}

    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitByRestrictionDecider
    {
        private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixPro matrix,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences
            )
        {
            _optimizationPreferences = optimizationPreferences;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(matrix, restrictionChecker);
        }

        public IList<DateOnly> OverLimit()
        {
            List<DateOnly> overallResult = new List<DateOnly>();
            overallResult.AddRange(PreferencesOverLimit());
            overallResult.AddRange(MustHavesOverLimit());
            overallResult.AddRange(RotationOverLimit());
            overallResult.AddRange(AvailabilitiesOverLimit());
            overallResult.AddRange(StudentAvailabilitiesOverLimit());

            return overallResult;
        }

        public IList<DateOnly> PreferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.PreferencesOverLimit(new Percent(_optimizationPreferences.General.PreferencesValue)).BrokenDays;
        }

        public IList<DateOnly> MustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.MustHavesOverLimit(new Percent(_optimizationPreferences.General.MustHavesValue)).BrokenDays;
        }

        public IList<DateOnly> RotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.RotationOverLimit(new Percent(_optimizationPreferences.General.RotationsValue)).BrokenDays;
        }

        public IList<DateOnly> AvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.AvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.AvailabilitiesValue)).BrokenDays;
        }

        public IList<DateOnly> StudentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return new List<DateOnly>();
            return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(new Percent(_optimizationPreferences.General.StudentAvailabilitiesValue)).BrokenDays;
        }
    }
}
