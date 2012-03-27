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

        public IList<DateOnly> PreferencesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckPreference);
        }

        public IList<DateOnly> MustHavesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckPreferenceMustHave);
        }

        public IList<DateOnly> RotationOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckRotations);
        }

        public IList<DateOnly> AvailabilitiesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckAvailability);
        }

        public IList<DateOnly> StudentAvailabilitiesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckStudentAvailability);
        }

        private IList<DateOnly> calculateReturn(Percent limit, Func<PermissionState> checkMethod)
        {
            double brokenLimit = calculateBrokenLimit(limit.Value);
            brokenReturn current = calculateBrokenPercentage(checkMethod);
            if (current.BrokenPercentage > brokenLimit)
                return current.BrokenDays;

            return new List<DateOnly>();
        }

        private brokenReturn calculateBrokenPercentage(Func<PermissionState> checkMethod)
        {
            int brokenDays = 0;
            int allDays = 0;
            IList<DateOnly> brokenDates = new List<DateOnly>();
            foreach (var scheduleDayPro in _matrix.EffectivePeriodDays)
            {
                _restrictionChecker.ScheduleDay = scheduleDayPro.DaySchedulePart();
                PermissionState permissionState = checkMethod();
                if (permissionState != PermissionState.None)
                    allDays++;
                if (permissionState == PermissionState.Broken)
                {
                    brokenDays++;
                    brokenDates.Add(scheduleDayPro.Day);
                }
            }

            double retPercentage;
            if (allDays == 0d)
                retPercentage = 0d;
            else
            {
                retPercentage = brokenDays/(double) allDays;
            }

            return new brokenReturn(brokenDates, retPercentage);
        }

        private static double calculateBrokenLimit(double fulFillValue)
        {
            return 1 - fulFillValue;
        }

        private class brokenReturn
        {
            private readonly IList<DateOnly> _brokenDays;
            private readonly double _brokenPercentage;

            public brokenReturn(IList<DateOnly> brokenDays, double brokenPercentage)
            {
                _brokenDays = brokenDays;
                _brokenPercentage = brokenPercentage;
            }

            public IList<DateOnly> BrokenDays
            {
                get { return _brokenDays; }
            }

            public double BrokenPercentage
            {
                get { return _brokenPercentage; }
            }
        }
    }
}
