using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IAnalyzePersonAccordingToAvailability
    {
		DateTimePeriod? AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo,  DateTimePeriod overtimeLayerLengthPeriod);
    }

    public class AnalyzePersonAccordingToAvailability : IAnalyzePersonAccordingToAvailability
    {
        private readonly AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;

        public AnalyzePersonAccordingToAvailability(AdjustOvertimeLengthBasedOnAvailability adjustOvertimeLengthBasedOnAvailability)
        {
            _adjustOvertimeLengthBasedOnAvailability = adjustOvertimeLengthBasedOnAvailability;
        }

	    public DateTimePeriod? AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly,
		    TimeZoneInfo timeZoneInfo, DateTimePeriod overtimeLayerLengthPeriod)
	    {

		    var ovrtimeCollectionForPerson = scheduleDay.OvertimeAvailablityCollection();
		    if (ovrtimeCollectionForPerson.Count == 0) return null;
		    var overtimePeriod =
			    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				    dateOnly.Date.Add(ovrtimeCollectionForPerson[0].StartTime.GetValueOrDefault()),
				    dateOnly.Date.Add(ovrtimeCollectionForPerson[0].EndTime.GetValueOrDefault()), timeZoneInfo);



		    var adjustedPeriod = _adjustOvertimeLengthBasedOnAvailability.AdjustOvertimeDuration(overtimePeriod, overtimeLayerLengthPeriod, scheduleDay);

		    return adjustedPeriod;
	    }
    }
}
