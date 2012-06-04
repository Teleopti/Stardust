using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling
{
	public class SchedulePeriodTargetTimeTimeCalculator : ISchedulePeriodTargetTimeCalculator
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

		public TimeSpan TargetTime(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			var contract = schedulePeriod.Contract;
			var employmentType = contract.EmploymentType;

			if (employmentType == EmploymentType.HourlyStaff)
				return contract.MinTimeSchedulePeriod;

			var target = PeriodTarget(schedulePeriod, scheduleDays);
			target = target.Add(TimeSpan.FromSeconds(target.TotalSeconds * schedulePeriod.Seasonality.Value));
			target = ApplyBalance(schedulePeriod, target);
			return target;
		}

		private static TimeSpan PeriodTarget(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays) {
			if (schedulePeriod.Contract.EmploymentType == EmploymentType.FixedStaffDayWorkTime)
				return PeriodTargetForStaffDayWorkTime(schedulePeriod, scheduleDays);
			else
				return schedulePeriod.PeriodTarget();
		}

		private static TimeSpan PeriodTargetForStaffDayWorkTime(IVirtualSchedulePeriod schedulePeriod, IEnumerable<IScheduleDay> scheduleDays)
		{
			TimeSpan target;
			var daysOff = 0;
			foreach (var day in scheduleDays)
			{
				var significant = day.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					daysOff++;
			}
			target =
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