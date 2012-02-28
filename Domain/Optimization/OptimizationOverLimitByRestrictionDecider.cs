using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitDecider
    {
        private readonly IOptimizationPreferences _optimizationPreferences;
        private readonly IRestrictionOverLimitDecider _restrictionOverLimitDecider; 

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences
            )
        {
            _optimizationPreferences = optimizationPreferences;
            _restrictionOverLimitDecider = new RestrictionOverLimitDecider(matrixOriginalStateContainer, restrictionChecker);
        }

        public bool OverLimit()
        {
            bool overallResult = preferencesOverLimit();
            if (overallResult)
                return true;

            overallResult = mustHavesOverLimit();
            if (overallResult)
                return true;

            overallResult = rotationOverLimit();
            if (overallResult)
                return true;

            overallResult = availabilitiesOverLimit();
            if (overallResult)
                return true;

            overallResult = studentAvailabilitiesOverLimit();
            if (overallResult)
                return true;

            return false;
        }

        private bool preferencesOverLimit()
        {
            if (!_optimizationPreferences.General.UsePreferences)
                return false;
            return _restrictionOverLimitDecider.PreferencesOverLimit(_optimizationPreferences.General.PreferencesValue);
        }

        private bool mustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return false;
            return _restrictionOverLimitDecider.MustHavesOverLimit(_optimizationPreferences.General.MustHavesValue);
        }

        private bool rotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return false;
            return _restrictionOverLimitDecider.RotationOverLimit(_optimizationPreferences.General.RotationsValue);
        }

        private bool availabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return false;
            return _restrictionOverLimitDecider.AvailabilitiesOverLimit(_optimizationPreferences.General.AvailabilitiesValue);
        }

        private bool studentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return false;
            return _restrictionOverLimitDecider.StudentAvailabilitiesOverLimit(_optimizationPreferences.General.StudentAvailabilitiesValue);
        }
    }
}
