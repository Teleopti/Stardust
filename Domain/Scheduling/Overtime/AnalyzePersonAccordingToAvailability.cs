using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IAnalyzePersonAccordingToAvailability
    {
		DateTimePeriod? AdjustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo,  DateTimePeriod overtimeLayerLengthPeriod);
    }

    public class AnalyzePersonAccordingToAvailability : IAnalyzePersonAccordingToAvailability
    {
        private readonly AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;

        public AnalyzePersonAccordingToAvailability(AdjustOvertimeLengthBasedOnAvailability adjustOvertimeLengthBasedOnAvailability)
        {
            _adjustOvertimeLengthBasedOnAvailability = adjustOvertimeLengthBasedOnAvailability;
        }

	    public DateTimePeriod? AdjustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly,
		    TimeZoneInfo timeZoneInfo, DateTimePeriod overtimeLayerLengthPeriod)
	    {

		    var overtimeAvailablityCollection = scheduleDay.OvertimeAvailablityCollection();
		    if (overtimeAvailablityCollection.Count == 0) return null;
		    var overtimePeriod =
			    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				    dateOnly.Date.Add(overtimeAvailablityCollection[0].StartTime.GetValueOrDefault()),
				    dateOnly.Date.Add(overtimeAvailablityCollection[0].EndTime.GetValueOrDefault()), timeZoneInfo);



		    var adjustedPeriod = _adjustOvertimeLengthBasedOnAvailability.AdjustOvertimeDuration(overtimePeriod, overtimeLayerLengthPeriod, scheduleDay);

		    return adjustedPeriod;
	    }
    }
}
