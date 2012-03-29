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

        public BrokenRestrictionsInfo PreferencesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckPreference);
        }

        public BrokenRestrictionsInfo MustHavesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckPreferenceMustHave);
        }

        public BrokenRestrictionsInfo RotationOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckRotations);
        }

        public BrokenRestrictionsInfo AvailabilitiesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckAvailability);
        }

        public BrokenRestrictionsInfo StudentAvailabilitiesOverLimit(Percent limit)
        {
            return calculateReturn(limit, _restrictionChecker.CheckStudentAvailability);
        }

        private BrokenRestrictionsInfo calculateReturn(Percent limit, Func<PermissionState> checkMethod)
        {
            double brokenLimit = calculateBrokenLimit(limit.Value);
            BrokenRestrictionsInfo current = calculateBrokenPercentage(checkMethod);
            if (current.BrokenPercentage.Value > brokenLimit)
                return new BrokenRestrictionsInfo(current.BrokenDays, current.BrokenPercentage);

            return new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent());
        }

        private BrokenRestrictionsInfo calculateBrokenPercentage(Func<PermissionState> checkMethod)
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

            return new BrokenRestrictionsInfo(brokenDates, new Percent(retPercentage));
        }

        private static double calculateBrokenLimit(double fulFillValue)
        {
            return 1 - fulFillValue;
        }
    }

    public class BrokenRestrictionsInfo
    {
        private readonly IList<DateOnly> _brokenDays;
        private readonly Percent _brokenPercentage;

        public BrokenRestrictionsInfo(IList<DateOnly> brokenDays, Percent brokenPercentage)
        {
            _brokenDays = brokenDays;
            _brokenPercentage = brokenPercentage;
        }

        public IList<DateOnly> BrokenDays
        {
            get { return _brokenDays; }
        }

        public Percent BrokenPercentage
        {
            get { return _brokenPercentage; }
        }
    }
}
