using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.Rules;
using Teleopti.Interfaces.Domain;

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

        public DayOffToTimeSpanExtractor(IExtractDayOffFromGivenWeek extractDayOffFromGivenWeek, IScheduleDayWorkShiftTimeExtractor scheduleDayWorkShiftTimeExtractor)
        {
            _extractDayOffFromGivenWeek = extractDayOffFromGivenWeek;
            _scheduleDayWorkShiftTimeExtractor = scheduleDayWorkShiftTimeExtractor;
        }

        public IDictionary<DateOnly, TimeSpan> GetDayOffWithTimeSpanAmongAWeek(DateOnlyPeriod week, IScheduleRange currentSchedules)
        {
            var scheduleDayList = currentSchedules.ScheduledDayCollection(week);
            var daysOffInProvidedWeek = _extractDayOffFromGivenWeek.GetDaysOff(scheduleDayList);
            var possibleDaysOffWithSpan = new Dictionary<DateOnly, TimeSpan>();
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
            if (isMissingShiftOnNeighbouringDays(previousScheduleDay, nextScheduleDay)) return TimeSpan.Zero;
            if (isNeighbouringDaysOff(previousScheduleDay, nextScheduleDay)) return null;
            
            var startEndTimeOfpreviousDay = _scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(previousScheduleDay);
            var startEndTimeOfNextDay = _scheduleDayWorkShiftTimeExtractor.ShiftStartEndTime(nextScheduleDay);
            if (startEndTimeOfpreviousDay.HasValue && startEndTimeOfNextDay.HasValue)
                return startEndTimeOfNextDay.Value.StartDateTime - startEndTimeOfpreviousDay.Value.EndDateTime;
            return null;
        }

        private bool isMissingShiftOnNeighbouringDays(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay)
        {
            if (previousScheduleDay.SignificantPart() == SchedulePartView.None  || nextScheduleDay.SignificantPart() == SchedulePartView.None)
                return true;
            return false;
        }

        private bool isNeighbouringDaysOff(IScheduleDay previousScheduleDay, IScheduleDay nextScheduleDay)
        {
            if (previousScheduleDay.SignificantPart() == SchedulePartView.DayOff || nextScheduleDay.SignificantPart() == SchedulePartView.DayOff)
                return true;
            return false;
        }
    }
}