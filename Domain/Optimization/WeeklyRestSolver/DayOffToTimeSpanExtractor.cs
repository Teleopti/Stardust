using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IDayOffToTimeSpanExtractor
    {
        IDictionary<DateOnly, TimeSpan> GetDayOffWithTimeSpanAmongAWeek(DateOnlyPeriod week, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix);
    }

    public class DayOffToTimeSpanExtractor : IDayOffToTimeSpanExtractor
    {
        private readonly IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
        private readonly IScheduleDayWorkShiftTimeExtractor _scheduleDayWorkShiftTimeExtractor;
        private readonly IVerifyWeeklyRestAroundDayOffSpecification  _verifyWeeklyRestAroundDayOffSpecification;
		private readonly IVerifyWeeklyRestNotLockedAroundDayOffSpecification _verifyWeeklyRestNotLockedAroundDayOffSpecification;

		public DayOffToTimeSpanExtractor(IExtractDayOffFromGivenWeek extractDayOffFromGivenWeek, 
			IScheduleDayWorkShiftTimeExtractor scheduleDayWorkShiftTimeExtractor, 
			IVerifyWeeklyRestAroundDayOffSpecification verifyWeeklyRestAroundDayOffSpecification,
			IVerifyWeeklyRestNotLockedAroundDayOffSpecification verifyWeeklyRestNotLockedAroundDayOffSpecification)
        {
            _extractDayOffFromGivenWeek = extractDayOffFromGivenWeek;
            _scheduleDayWorkShiftTimeExtractor = scheduleDayWorkShiftTimeExtractor;
            _verifyWeeklyRestAroundDayOffSpecification = verifyWeeklyRestAroundDayOffSpecification;
			_verifyWeeklyRestNotLockedAroundDayOffSpecification = verifyWeeklyRestNotLockedAroundDayOffSpecification;
        }

		public IDictionary<DateOnly, TimeSpan> GetDayOffWithTimeSpanAmongAWeek(DateOnlyPeriod week, IScheduleRange currentSchedules, IScheduleMatrixPro scheduleMatrix)
        {
            var possibleDaysOffWithSpan = new Dictionary<DateOnly, TimeSpan>();
            var scheduleDayList = currentSchedules.ScheduledDayCollection(week);
            var daysOffInProvidedWeek = _extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList);
            if (daysOffInProvidedWeek.IsEmpty())
                return possibleDaysOffWithSpan;
            if (!_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(daysOffInProvidedWeek, currentSchedules) ||
				!_verifyWeeklyRestNotLockedAroundDayOffSpecification.IsSatisfy(daysOffInProvidedWeek, currentSchedules, scheduleMatrix))
                return possibleDaysOffWithSpan;
            foreach(var dayOffDate in daysOffInProvidedWeek )
            {
                var longestSpanWithConsecutiveDays = getTimeSpanOnConsecutiveDays(dayOffDate, currentSchedules);
                if (!longestSpanWithConsecutiveDays.HasValue ) continue;
                possibleDaysOffWithSpan.Add(dayOffDate, longestSpanWithConsecutiveDays.Value);
            }
            return possibleDaysOffWithSpan;

        }

        private TimeSpan? getTimeSpanOnConsecutiveDays(DateOnly dayOffDate, IScheduleRange currentSchedules)
        {
            var previousScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(-1));
            var nextScheduleDay = currentSchedules.ScheduledDay(dayOffDate.AddDays(1));
            
            var startEndTimeOfpreviousDay = _scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(previousScheduleDay);
            var startEndTimeOfNextDay = _scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(nextScheduleDay);
            if (startEndTimeOfpreviousDay.HasValue && startEndTimeOfNextDay.HasValue)
                return startEndTimeOfNextDay.Value.StartDateTime - startEndTimeOfpreviousDay.Value.EndDateTime;
            return null;
        }
    }
}