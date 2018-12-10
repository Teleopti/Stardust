using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulePeriodTargetTimeCalculator : ISchedulePeriodTargetTimeCalculator
    {
        public TimePeriod TargetWithTolerance(IScheduleMatrixPro matrix)
        {
			if (matrix == null) throw new ArgumentNullException(nameof(matrix));
        	return TargetTimeWithTolerance(matrix.SchedulePeriod, MatrixScheduleDays(matrix));
        }

		public TimeSpan TargetTime(IScheduleMatrixPro matrix)
    	{
			if (matrix == null) throw new ArgumentNullException(nameof(matrix));
			return TargetTime(matrix.SchedulePeriod, MatrixScheduleDays(matrix));
    	}

		private static IEnumerable<IScheduleDay> MatrixScheduleDays(IScheduleMatrixPro matrix)
		{
			IEnumerable<IScheduleDayPro> effectivePeriodDays = matrix.EffectivePeriodDays;
			effectivePeriodDays = effectivePeriodDays ?? new IScheduleDayPro[] { };
			return from d in effectivePeriodDays select d.DaySchedulePart();
		}

		public TimeSpan TargetTime(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			var contract = virtualSchedulePeriod.Contract;
			var employmentType = contract.EmploymentType;

			if (employmentType == EmploymentType.HourlyStaff)
				return contract.MinTimeSchedulePeriod;

			var target = PeriodTarget(virtualSchedulePeriod, scheduleDays);
			target = ApplySeasonality(virtualSchedulePeriod, target);
			target = ApplyBalance(virtualSchedulePeriod, target);
			return target;
		}

		public TimePeriod TargetTimeWithTolerance(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			var contract = virtualSchedulePeriod.Contract;
			var employmentType = contract.EmploymentType;

			if (employmentType == EmploymentType.HourlyStaff)
			{
				var weeks = virtualSchedulePeriod.DateOnlyPeriod.CalendarWeeksAffected(virtualSchedulePeriod.Person.FirstDayOfWeek);
				var maxTime = TimeSpan.FromSeconds(contract.WorkTimeDirective.MaxTimePerWeek.TotalSeconds * weeks);
				if (maxTime < virtualSchedulePeriod.MinTimeSchedulePeriod)
					maxTime = virtualSchedulePeriod.MinTimeSchedulePeriod;
				return new TimePeriod(virtualSchedulePeriod.MinTimeSchedulePeriod, maxTime);
			}

			var target = TargetTime(virtualSchedulePeriod, scheduleDays);

			return ApplyTolerance(contract, target);
		}

		private static TimeSpan PeriodTarget(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			if (schedulePeriod.Contract.EmploymentType == EmploymentType.FixedStaffDayWorkTime)
				return PeriodTargetForStaffDayWorkTime(schedulePeriod, scheduleDays);

			return schedulePeriod.PeriodTarget();
		}

		private static TimeSpan PeriodTargetForStaffDayWorkTime(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			var daysOff = (
							from d in scheduleDays
							let restrictions = d.RestrictionCollection() ?? new IRestrictionBase[] { }
							let dayOffPreferenceRestrictions = from r in restrictions.OfType<IPreferenceRestriction>()
															   where r.DayOffTemplate != null
															   select r
							let significant = d.SignificantPart()
							where
								significant == SchedulePartView.DayOff ||
								significant == SchedulePartView.ContractDayOff ||
								dayOffPreferenceRestrictions.Any()
							select d
						   ).Count();

			var workDays = schedulePeriod.DateOnlyPeriod.DayCount() - daysOff;

			var periodTarget = TimeSpan.FromSeconds(workDays * schedulePeriod.AverageWorkTimePerDay.TotalSeconds);

			return periodTarget;
		}

		private static TimeSpan ApplySeasonality(IVirtualSchedulePeriod virtualSchedulePeriod, TimeSpan target)
		{
			return target.Add(TimeSpan.FromSeconds(target.TotalSeconds * virtualSchedulePeriod.Seasonality.Value));
		}

		private static TimeSpan ApplyBalance(IVirtualSchedulePeriod schedulePeriod, TimeSpan time)
		{
			time = time.Add(schedulePeriod.Extra);
			time = time.Add(schedulePeriod.BalanceOut);
			time = time.Subtract(schedulePeriod.BalanceIn);
			return time;
		}

		private static TimePeriod ApplyTolerance(IContract contract, TimeSpan target)
		{
			var min = target.Subtract(contract.NegativePeriodWorkTimeTolerance);
			var max = target.Add(contract.PositivePeriodWorkTimeTolerance);
			return new TimePeriod(min, max);
		}
	}

}