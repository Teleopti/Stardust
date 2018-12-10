using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.DayOffScheduling;

namespace Teleopti.Ccc.Domain.ResourceCalculation
{
	public class DayOffsInPeriodCalculator : IDayOffsInPeriodCalculator
	{
		public IList<IScheduleDay> CountDayOffsOnPeriod(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			IList<IScheduleDay> dayOffDays = new List<IScheduleDay>();

			var range = scheduleDictionary[virtualSchedulePeriod.Person];
			foreach (var scheduleDay in range.ScheduledDayCollection(virtualSchedulePeriod.DateOnlyPeriod))
			{
				SchedulePartView significant = scheduleDay.SignificantPart();
				if (significant == SchedulePartView.DayOff || significant == SchedulePartView.ContractDayOff)
					dayOffDays.Add(scheduleDay);
			}

			return dayOffDays;
		}

		public bool HasCorrectNumberOfDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod, out int targetDaysOff, out IList<IScheduleDay> dayOffsNow)
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
			dayOffsNow = CountDayOffsOnPeriod(scheduleDictionary, virtualSchedulePeriod);


			if (dayOffsNow.Count >= targetDaysOff - contract.NegativeDayOffTolerance && dayOffsNow.Count <= targetDaysOff + contract.PositiveDayOffTolerance)
				return true;

			return false;
		}

        public bool OutsideOrAtMinimumTargetDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;

	        if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            int targetDaysOff = virtualSchedulePeriod.DaysOff();
            IList<IScheduleDay> dayOffsNow = CountDayOffsOnPeriod(scheduleDictionary, virtualSchedulePeriod);

            return (dayOffsNow.Count <= targetDaysOff - contract.NegativeDayOffTolerance);
        }

        public bool OutsideOrAtMaximumTargetDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod)
        {
            var contract = virtualSchedulePeriod.Contract;
            var employmentType = contract.EmploymentType;

	        if (employmentType == EmploymentType.HourlyStaff)
            {
                return false;
            }

            int targetDaysOff = virtualSchedulePeriod.DaysOff();
            IList<IScheduleDay> dayOffsNow = CountDayOffsOnPeriod(scheduleDictionary, virtualSchedulePeriod);

            return (dayOffsNow.Count >= targetDaysOff + contract.PositiveDayOffTolerance);
        }

		public IList<DayOffOnPeriod> WeekPeriodsSortedOnDayOff(IScheduleMatrixPro scheduleMatrixPro)
		{
			var weekPeriod = new DateOnlyPeriod();
			var weekPeriods = new List<DayOffOnPeriod>();

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

		public DayOffOnPeriod CountDayOffsOnPeriod(IScheduleMatrixPro scheduleMatrixPro, DateOnlyPeriod period)
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


	public interface IDaysOffInPeriodValidatorForBlock
	{
		bool HasCorrectNumberOfDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod);
	}


	public class DaysOffInPeriodValidatorForBlock : IDaysOffInPeriodValidatorForBlock
	{
		private readonly IDayOffsInPeriodCalculator _dayOffsInPeriodCalculator;

		public DaysOffInPeriodValidatorForBlock(IDayOffsInPeriodCalculator dayOffsInPeriodCalculator)
		{
			_dayOffsInPeriodCalculator = dayOffsInPeriodCalculator;
		}

		public bool HasCorrectNumberOfDaysOff(IScheduleDictionary scheduleDictionary, IVirtualSchedulePeriod virtualSchedulePeriod)
		{
			return _dayOffsInPeriodCalculator.HasCorrectNumberOfDaysOff(scheduleDictionary, virtualSchedulePeriod, out int _,
				out IList<IScheduleDay> _);
		}
	}
}