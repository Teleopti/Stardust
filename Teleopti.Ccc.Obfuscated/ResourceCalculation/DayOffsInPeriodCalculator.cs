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

        public int CountDayOffsOnPeriod(IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			int countDayOffs = 0;
            var range = ScheduleDictionary[virtualSchedulePeriod.Person];
			foreach (var scheduleDay in range.ScheduledDayCollection(virtualSchedulePeriod.DateOnlyPeriod))
			{
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					countDayOffs += 1;
			}
			return countDayOffs;
		}

		public bool HasCorrectNumberOfDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out int dayOffsNow)
		{
            var contract = virtualSchedulePeriod.Contract;
		    var employmentType = contract.EmploymentType;

			if (employmentType == EmploymentType.HourlyStaff)
			{
				targetDaysOff = 0;
				dayOffsNow = 0;
				return true;
			}

			targetDaysOff = virtualSchedulePeriod.DaysOff();
			dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);


            if (dayOffsNow >= targetDaysOff - contract.NegativeDayOffTolerance && dayOffsNow <= targetDaysOff + contract.PositiveDayOffTolerance)
                return true;

			return false;
		}

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool OutsideOrAtMinimumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;
            int targetDaysOff;
            int dayOffsNow;

            if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            targetDaysOff = virtualSchedulePeriod.DaysOff();
            dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);

            return (dayOffsNow <= targetDaysOff - contract.NegativeDayOffTolerance);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0")]
        public bool OutsideOrAtMaximumTargetDaysOff(IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;
            int targetDaysOff;
            int dayOffsNow;

            if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            targetDaysOff = virtualSchedulePeriod.DaysOff();
            dayOffsNow = CountDayOffsOnPeriod(virtualSchedulePeriod);

            return (dayOffsNow >= targetDaysOff + contract.PositiveDayOffTolerance);
        }
	}
}