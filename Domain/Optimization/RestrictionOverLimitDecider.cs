using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RestrictionOverLimitDecider : IRestrictionOverLimitDecider
    {
        private readonly IScheduleMatrixPro _matrix;
        private readonly ICheckerRestriction _restrictionChecker;

        public RestrictionOverLimitDecider(
            IScheduleMatrixPro matrix,
            ICheckerRestriction restrictionChecker
            )
        {
            _matrix = matrix;
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
            return currentValue > brokenLimit;
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
            foreach (var scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                _restrictionChecker.ScheduleDay = scheduleDayPro.DaySchedulePart();
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

        private static double calculateBrokenLimit(double fulFillValue)
        {
            return 1 - fulFillValue;
        }
    }
}
