using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulePeriodTargetTimeCalculator : ISchedulePeriodTargetTimeCalculator
    {

        public TimePeriod TargetWithTolerance(IScheduleMatrixPro matrix)
        {
            if (matrix == null) throw new ArgumentNullException("matrix");
            IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
            var contract = schedulePeriod.Contract;
           
            EmploymentType empType = contract.EmploymentType;

            if (empType == EmploymentType.HourlyStaff)
            {
                int weeks = schedulePeriod.DateOnlyPeriod.CalendarWeeksAffected(matrix.Person.FirstDayOfWeek);
                TimeSpan maxTime =
                    TimeSpan.FromSeconds(contract.WorkTimeDirective.MaxTimePerWeek.TotalSeconds*weeks);
                if (maxTime < schedulePeriod.MinTimeSchedulePeriod)
                    maxTime = schedulePeriod.MinTimeSchedulePeriod;
                return new TimePeriod(schedulePeriod.MinTimeSchedulePeriod, maxTime);
            }

        	TimeSpan target = TargetTime(matrix);
            return new TimePeriod(target.Subtract(contract.NegativePeriodWorkTimeTolerance),
                                  target.Add(contract.PositivePeriodWorkTimeTolerance));

        }

    	public TimeSpan TargetTime(IScheduleMatrixPro matrix)
    	{
			if (matrix == null) throw new ArgumentNullException("matrix");
    		IEnumerable<IScheduleDayPro> effectivePeriodDays = matrix.EffectivePeriodDays;
			effectivePeriodDays = effectivePeriodDays ?? new IScheduleDayPro[] { };
			var scheduleDays = (from d in effectivePeriodDays select d.DaySchedulePart());
			return TargetTime(matrix.SchedulePeriod, scheduleDays);
    	}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
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

		private static TimeSpan ApplySeasonality(IVirtualSchedulePeriod virtualSchedulePeriod, TimeSpan target)
		{
			return target.Add(TimeSpan.FromSeconds(target.TotalSeconds * virtualSchedulePeriod.Seasonality.Value));
		}

		private static TimeSpan PeriodTarget(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays) {
			if (schedulePeriod.Contract.EmploymentType == EmploymentType.FixedStaffDayWorkTime)
				return PeriodTargetForStaffDayWorkTime(schedulePeriod, scheduleDays);
			else
				return schedulePeriod.PeriodTarget();
		}

		private static TimeSpan PeriodTargetForStaffDayWorkTime(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			var daysOff = 0;
			foreach (var day in scheduleDays)
			{
				var significant = day.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					daysOff++;
			}
			var target =
				TimeSpan.FromSeconds((schedulePeriod.DateOnlyPeriod.DayCount() - daysOff)*
				                     schedulePeriod.AverageWorkTimePerDay.TotalSeconds);
			return target;
		}

		private static TimeSpan ApplyBalance(IVirtualSchedulePeriod schedulePeriod, TimeSpan time)
		{
			time = time.Add(schedulePeriod.Extra);
			time = time.Add(schedulePeriod.BalanceOut);
			time = time.Subtract(schedulePeriod.BalanceIn);
			return time;
		}
	}
}