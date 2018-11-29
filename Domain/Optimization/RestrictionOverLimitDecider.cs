using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization
{
    public class RestrictionOverLimitDecider : IRestrictionOverLimitDecider
    {

        private readonly ICheckerRestriction _restrictionChecker;

        public RestrictionOverLimitDecider(ICheckerRestriction restrictionChecker)
        {
            _restrictionChecker = restrictionChecker;
        }

		public BrokenRestrictionsInfo PreferencesOverLimit(Percent limit, IScheduleMatrixPro matrix)
        {
			return calculateReturn(limit, _restrictionChecker.CheckPreference, matrix);
        }

		public BrokenRestrictionsInfo MustHavesOverLimit(Percent limit, IScheduleMatrixPro matrix)
        {
			return calculateReturn(limit, _restrictionChecker.CheckPreferenceMustHave, matrix);
        }

		public BrokenRestrictionsInfo RotationOverLimit(Percent limit, IScheduleMatrixPro matrix)
        {
			return calculateReturn(limit, _restrictionChecker.CheckRotations, matrix);
        }

		public BrokenRestrictionsInfo AvailabilitiesOverLimit(Percent limit, IScheduleMatrixPro matrix)
        {
			return calculateReturn(limit, _restrictionChecker.CheckAvailability, matrix);
        }

		public BrokenRestrictionsInfo StudentAvailabilitiesOverLimit(Percent limit, IScheduleMatrixPro matrix)
        {
            return calculateReturn(limit, _restrictionChecker.CheckStudentAvailability, matrix);
        }

		private BrokenRestrictionsInfo calculateReturn(Percent limit, Func<IScheduleDay, PermissionState> checkMethod, IScheduleMatrixPro matrix)
        {
            double brokenLimit = calculateBrokenLimit(limit.Value);
            BrokenRestrictionsInfo current = calculateBrokenPercentage(checkMethod, matrix);
            if (current.BrokenPercentage.Value > brokenLimit)
                return new BrokenRestrictionsInfo(current.BrokenDays, current.BrokenPercentage);

            return new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent());
        }

		private BrokenRestrictionsInfo calculateBrokenPercentage(Func<IScheduleDay, PermissionState> checkMethod, IScheduleMatrixPro matrix)
		{
			int brokenDays = 0;
			int allDays = 0;
			IList<DateOnly> brokenDates = new List<DateOnly>();
			foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
			{
				var permissionState = checkMethod(scheduleDayPro.DaySchedulePart());
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
				retPercentage = brokenDays / (double)allDays;
			}

			return new BrokenRestrictionsInfo(brokenDates, new Percent(retPercentage));
		}

		//private BrokenRestrictionsInfo calculateReturnOld(Percent limit, Func<PermissionState> checkMethod, IScheduleMatrixPro matrix)
		//{
		//	double brokenLimit = calculateBrokenLimit(limit.Value);
		//	BrokenRestrictionsInfo current = calculateBrokenPercentageOld(checkMethod, matrix);
		//	if (current.BrokenPercentage.Value > brokenLimit)
		//		return new BrokenRestrictionsInfo(current.BrokenDays, current.BrokenPercentage);

		//	return new BrokenRestrictionsInfo(new List<DateOnly>(), new Percent());
		//}

		//private BrokenRestrictionsInfo calculateBrokenPercentageOld(Func<PermissionState> checkMethod, IScheduleMatrixPro matrix)
		//{
		//	int brokenDays = 0;
		//	int allDays = 0;
		//	IList<DateOnly> brokenDates = new List<DateOnly>();
		//	foreach (var scheduleDayPro in matrix.EffectivePeriodDays)
		//	{
		//		_restrictionChecker.ScheduleDay = scheduleDayPro.DaySchedulePart();

		//		PermissionState permissionState = checkMethod();
		//		if (permissionState != PermissionState.None)
		//			allDays++;
		//		if (permissionState == PermissionState.Broken)
		//		{
		//			brokenDays++;
		//			brokenDates.Add(scheduleDayPro.Day);
		//		}
		//	}

		//	double retPercentage;
		//	if (allDays == 0d)
		//		retPercentage = 0d;
		//	else
		//	{
		//		retPercentage = brokenDays/(double) allDays;
		//	}

		//	return new BrokenRestrictionsInfo(brokenDates, new Percent(retPercentage));
		//}

        private static double calculateBrokenLimit(double fulFillValue)
        {
			//have to use decimal to avoid floating point rounding errors
            return (double)(1 - (decimal)fulFillValue);
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
