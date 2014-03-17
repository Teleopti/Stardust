using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IAnalyzePersonAccordingToAvailability
    {
        TimeSpan AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo, TimeSpan overtimeLayerLength, DateTime shiftEndTime);
    }

    public class AnalyzePersonAccordingToAvailability : IAnalyzePersonAccordingToAvailability
    {
        private readonly AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;

        public AnalyzePersonAccordingToAvailability(AdjustOvertimeLengthBasedOnAvailability adjustOvertimeLengthBasedOnAvailability)
        {
            _adjustOvertimeLengthBasedOnAvailability = adjustOvertimeLengthBasedOnAvailability;
        }

        public TimeSpan AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo, TimeSpan overtimeLayerLength, DateTime shiftEndTime)
        {
            var ovrtimeCollectionForPerson = scheduleDay.OvertimeAvailablityCollection();
            if (ovrtimeCollectionForPerson.Count == 0) return TimeSpan.Zero;
            var overtimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly.Date.Add(ovrtimeCollectionForPerson[0].StartTime.GetValueOrDefault()),
                                                                dateOnly.Date.Add(ovrtimeCollectionForPerson[0].EndTime.GetValueOrDefault( )),timeZoneInfo);
            return  _adjustOvertimeLengthBasedOnAvailability.AdjustOvertimeDuration(overtimePeriod, overtimeLayerLength, shiftEndTime);
        }
    }
}
