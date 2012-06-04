using System;
using System.Collections.Generic;
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
			IVirtualSchedulePeriod schedulePeriod = matrix.SchedulePeriod;
    	    var contract = schedulePeriod.Contract;
			//IContract contract = schedulePeriod.PersonPeriod.PersonContract.Contract;
			EmploymentType empType = contract.EmploymentType;

			TimeSpan target;
			if (empType == EmploymentType.HourlyStaff)
			{
				return contract.MinTimeSchedulePeriod;
			}

			//PeriodTarget+(Extra+BalanceOut-BalanceIn)
			TimeSpan balanceAdjustment =
				schedulePeriod.Extra.Add(schedulePeriod.BalanceOut).Subtract(schedulePeriod.BalanceIn);
			if (empType == EmploymentType.FixedStaffDayWorkTime)
			{
				int daysOff = 0;
				foreach (var day in matrix.EffectivePeriodDays)
				{
					SchedulePartView significant = day.DaySchedulePart().SignificantPart();
					if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
						daysOff++;

				}
				target =
					TimeSpan.FromSeconds((schedulePeriod.DateOnlyPeriod.DayCount() - daysOff) *
										 schedulePeriod.AverageWorkTimePerDay.TotalSeconds);
                target = target.Add(TimeSpan.FromSeconds(target.TotalSeconds * schedulePeriod.Seasonality.Value));
				target = target.Add(balanceAdjustment);
				return target;
			}

			target = schedulePeriod.PeriodTarget();
            target = target.Add(TimeSpan.FromSeconds(target.TotalSeconds * schedulePeriod.Seasonality.Value));
			target = target.Add(balanceAdjustment);
    		return target;
    	}

		public TimeSpan TargetTime(IVirtualSchedulePeriod virtualSchedulePeriod, IEnumerable<IScheduleDay> scheduleDays) { return TimeSpan.Zero; }
    }
}