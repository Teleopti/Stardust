using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class DayOffsInPeriodCalculator : IDayOffsInPeriodCalculator
	{
	    private readonly ISchedulingResultStateHolder _resultStateHolder;
	    
		public DayOffsInPeriodCalculator(ISchedulingResultStateHolder resultStateHolder)
		{
		    _resultStateHolder = resultStateHolder;		    
		}

        private IScheduleDictionary ScheduleDictionary { get { return _resultStateHolder.Schedules; } }

		public IList<IScheduleDay> CountDayOffsOnPeriod(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			IList<IScheduleDay> dayOffDays = new List<IScheduleDay>();

			var range = ScheduleDictionary[virtualSchedulePeriod.Person];
			foreach (var scheduleDay in range.ScheduledDayCollection(virtualSchedulePeriod.DateOnlyPeriod))
			{
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					dayOffDays.Add(scheduleDay);
			}

			return dayOffDays;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out IList<IScheduleDay> dayOffsNow)
		{
			var contract = virtualSchedulePeriod.Contract;
			var employmentType = contract.EmploymentType;

			if (employmentType == EmploymentType.HourlyStaff)
			{
				targetDaysOff = 0;
				dayOffsNow = new List<IScheduleDay>();
				return true;
			}

			targetDaysOff = virtualSchedulePeriod.DaysOff();
			dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);


			if (dayOffsNow.Count >= targetDaysOff - contract.NegativeDayOffTolerance && dayOffsNow.Count <= targetDaysOff + contract.PositiveDayOffTolerance)
				return true;

			return false;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool OutsideOrAtMinimumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;
            int targetDaysOff;
			IList<IScheduleDay> dayOffsNow;

            if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            targetDaysOff = virtualSchedulePeriod.DaysOff();
            dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);

            return (dayOffsNow.Count <= targetDaysOff - contract.NegativeDayOffTolerance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool OutsideOrAtMaximumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;
            int targetDaysOff;
			IList<IScheduleDay> dayOffsNow;

            if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            targetDaysOff = virtualSchedulePeriod.DaysOff();
            dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);

            return (dayOffsNow.Count >= targetDaysOff + contract.PositiveDayOffTolerance);
        }

		public IList<IDayOffOnPeriod> WeekPeriodsSortedOnDayOff(IScheduleMatrixPro scheduleMatrixPro)
		{
			var weekPeriod = new DateOnlyPeriod();
			var weekPeriods = new List<IDayOffOnPeriod>();

			foreach (var scheduleDayPro in scheduleMatrixPro.UnlockedDays)
			{
				var weekPeriodForDay = DateHelper.GetWeekPeriod(scheduleDayPro.Day, CultureInfo.CurrentCulture);
				if (weekPeriod.Equals(weekPeriodForDay)) continue;

				var periodDayOff = CountDayOffsOnPeriod(scheduleMatrixPro, weekPeriodForDay);
				weekPeriods.Add(periodDayOff);
				

				weekPeriod = weekPeriodForDay;
			}

			var sortedOnMinList = weekPeriods.OrderBy(p => p.DaysOffCount).ToList();
			return sortedOnMinList;
		}

		public IDayOffOnPeriod CountDayOffsOnPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod period)
		{
			var count = 0;
			IList<IScheduleDay> scheduleDays = new List<IScheduleDay>();

			foreach (var scheduleDayPro in scheduleMatrixPro.OuterWeeksPeriodDays)
			{
				if(!period.Contains(scheduleDayPro.Day)) continue;
				var scheduleDay = scheduleDayPro.DaySchedulePart();
				var significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					count++;

				if(scheduleMatrixPro.UnlockedDays.Contains(scheduleDayPro))
					scheduleDays.Add(scheduleDay);
			}

			var dayOffOnPeriod = new DayOffOnPeriod(period, scheduleDays, count);

			return dayOffOnPeriod;
		}
	}
}