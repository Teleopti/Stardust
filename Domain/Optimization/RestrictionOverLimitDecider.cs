using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RestrictionOverLimitDecider : IRestrictionOverLimitDecider
    {
        private readonly IScheduleMatrixOriginalStateContainer _matrixOriginalStateContainer;
        private readonly ICheckerRestriction _restrictionChecker;

        public RestrictionOverLimitDecider(
            IScheduleMatrixOriginalStateContainer matrixOriginalStateContainer,
            ICheckerRestriction restrictionChecker
            )
        {
            _matrixOriginalStateContainer = matrixOriginalStateContainer;
            _restrictionChecker = restrictionChecker;
        }

        public bool PreferencesOverLimit(double limit)
        {
            double brokenLimit = calculateBrokenLimit(limit);
            double currentValue = calculateBrokenPreferencesPercentage();
            return currentValue > brokenLimit;
        }

        private double calculateBrokenPreferencesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckPreference));
        }

        public bool MustHavesOverLimit(double limit)
        {
            double brokenLimit = calculateBrokenLimit(limit);
            double currentValue = calculateBrokenMustHavesPercentage();
            return currentValue > brokenLimit;
        }

        private double calculateBrokenMustHavesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckPreferenceMustHave));
        }

        public bool RotationOverLimit(double limit)
        {
            double brokenLimit = calculateBrokenLimit(limit);
            double currentValue = calculateBrokenRotationPercentage();
            return currentValue > limit;
        }

        private double calculateBrokenRotationPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckRotations));
        }

        public bool AvailabilitiesOverLimit(double limit)
        {
            double brokenLimit = calculateBrokenLimit(limit);
            double currentValue = calculateBrokenAvailabilitiesPercentage();
            return currentValue > brokenLimit;
        }

        private double calculateBrokenAvailabilitiesPercentage()
        {
            return calculateBrokenPercentage(new Func<PermissionState>(_restrictionChecker.CheckAvailability));
        }

        public bool StudentAvailabilitiesOverLimit(double limit)
        {
            double brokenLimit = calculateBrokenLimit(limit);
            double currentValue = calculateBrokenStudentAvailabilitiesPercentage();
            return currentValue > brokenLimit;
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

        private double calculateBrokenLimit(double fulFillValue)
        {
            return 1 - fulFillValue;
        }
    }
}
