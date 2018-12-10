using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Collection;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IDayOffToTimeSpanExtractor
    {
        IDictionary<DateOnly, TimeSpan> GetDayOffWithTimeSpanAmongAWeek(DateOnlyPeriod week, IScheduleRange currentSchedules);
    }

    public class DayOffToTimeSpanExtractor : IDayOffToTimeSpanExtractor
    {
        private readonly IExtractDayOffFromGivenWeek _extractDayOffFromGivenWeek;
        private readonly IScheduleDayWorkShiftTimeExtractor _scheduleDayWorkShiftTimeExtractor;
        private readonly IVerifyWeeklyRestAroundDayOffSpecification  _verifyWeeklyRestAroundDayOffSpecification;

		public DayOffToTimeSpanExtractor(IExtractDayOffFromGivenWeek extractDayOffFromGivenWeek, 
			IScheduleDayWorkShiftTimeExtractor scheduleDayWorkShiftTimeExtractor, 
			IVerifyWeeklyRestAroundDayOffSpecification verifyWeeklyRestAroundDayOffSpecification)
        {
            _extractDayOffFromGivenWeek = extractDayOffFromGivenWeek;
            _scheduleDayWorkShiftTimeExtractor = scheduleDayWorkShiftTimeExtractor;
            _verifyWeeklyRestAroundDayOffSpecification = verifyWeeklyRestAroundDayOffSpecification;
        }

		public IDictionary<DateOnly, TimeSpan> GetDayOffWithTimeSpanAmongAWeek(DateOnlyPeriod week, IScheduleRange currentSchedules)
        {
            var possibleDaysOffWithSpan = new Dictionary<DateOnly, TimeSpan>();
            var scheduleDayList = currentSchedules.ScheduledDayCollection(week);
            var daysOffInProvidedWeek = _extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList);
            if (daysOffInProvidedWeek.IsEmpty())
                return possibleDaysOffWithSpan;
            if (!_verifyWeeklyRestAroundDayOffSpecification.IsSatisfy(daysOffInProvidedWeek, currentSchedules))
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