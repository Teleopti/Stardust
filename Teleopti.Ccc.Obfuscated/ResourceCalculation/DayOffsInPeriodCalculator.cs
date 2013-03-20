using System.Collections.Generic;
using System.Globalization;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Obfuscated.ResourceCalculation
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

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "1"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
		public IScheduleDay DayOffInScheduleDayWeek(IScheduleDay scheduleDay, IList<IScheduleDay> dayOffDays)
		{
			var week = DateHelper.WeekNumber(scheduleDay.DateOnlyAsPeriod.DateOnly.Date, CultureInfo.CurrentCulture);

			foreach (var dayOffDay in dayOffDays)
			{
				var dayOffWeek = DateHelper.WeekNumber(dayOffDay.DateOnlyAsPeriod.DateOnly.Date, CultureInfo.CurrentCulture);
				if (dayOffWeek == week)
				{
					return dayOffDay;
				}
			}

			return null;	
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
	}
}