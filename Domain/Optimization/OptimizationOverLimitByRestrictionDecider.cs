using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class OptimizationOverLimitByRestrictionDecider : IOptimizationOverLimitDecider
    {
        private readonly IScheduleMatrixOriginalStateContainer _matrixOriginalStateContainer;
        private readonly ICheckerRestriction _restrictionChecker;
        private readonly IOptimizationPreferences _optimizationPreferences;

        public OptimizationOverLimitByRestrictionDecider(
            IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer,
            ICheckerRestriction restrictionChecker,
            IOptimizationPreferences optimizationPreferences
            )
        {
            _matrixOriginalStateContainer = matrixOriginalStateContainer;
            _restrictionChecker = restrictionChecker;
            _optimizationPreferences = optimizationPreferences;
        }

        public bool OverLimit(ILogWriter logWriter)
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
            double limit = _optimizationPreferences.General.PreferencesValue;
            double currentValue = calculateBrokenPreferencesPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenPreferencesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckPreference));
        }

        private bool mustHavesOverLimit()
        {
            if (!_optimizationPreferences.General.UseMustHaves)
                return false;
            double limit = _optimizationPreferences.General.MustHavesValue;
            double currentValue = calculateBrokenMustHavesPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenMustHavesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckPreferenceMustHave));
        }

        private bool rotationOverLimit()
        {
            if (!_optimizationPreferences.General.UseRotations)
                return false;
            double limit = _optimizationPreferences.General.RotationsValue;
            double currentValue = calculateBrokenRotationPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenRotationPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckRotations));
        }

        private bool availabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseAvailabilities)
                return false;
            double limit = _optimizationPreferences.General.AvailabilitiesValue;
            double currentValue = calculateBrokenAvailabilitiesPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenAvailabilitiesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckAvailability));
        }

        private bool studentAvailabilitiesOverLimit()
        {
            if (!_optimizationPreferences.General.UseStudentAvailabilities)
                return false;
            double limit = _optimizationPreferences.General.StudentAvailabilitiesValue;
            double currentValue = calculateBrokenStudentAvailabilitiesPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenStudentAvailabilitiesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckStudentAvailability));
        }

        private double calculateBrokenPercentage(Func<PermissionState> checkMethod)
        {
            int brokenDays = 0;
            int allDays = 0;
            IDictionary<DateOnly, IScheduleDay> originalState
                = _matrixOriginalStateContainer.OldPeriodDaysState;
            foreach (IScheduleDay scheduleDay in originalState.Values)
            {
                _restrictionChecker.ScheduleDay = scheduleDay;
                PermissionState permissionState = checkMethod();
                if (permissionState != PermissionState.None)
                    allDays++;
                if (permissionState == PermissionState.Broken)
                    brokenDays++;
            }
            if (allDays == 0d)
                return 0d;
            return (double)brokenDays / (double)allDays;
        }
    }
}
