using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class AnalyzePersonAccordingToAvailability
    {
        private readonly AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;

        public AnalyzePersonAccordingToAvailability(AdjustOvertimeLengthBasedOnAvailability adjustOvertimeLengthBasedOnAvailability)
        {
            _adjustOvertimeLengthBasedOnAvailability = adjustOvertimeLengthBasedOnAvailability;
        }

	    public DateTimePeriod? AdjustOvertimeAvailability(IScheduleDay scheduleDay, DateTimePeriod overtimeLayerLengthPeriod)
	    {
		    var timeZoneInfo = scheduleDay.Person.PermissionInformation.DefaultTimeZone();
		    var dateOnly = scheduleDay.DateOnlyAsPeriod.DateOnly;
			var overtimeAvailablityCollection = scheduleDay.OvertimeAvailablityCollection();
		    if (overtimeAvailablityCollection.Length == 0) return null;
		    var overtimePeriod =
			    TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(
				    dateOnly.Date.Add(overtimeAvailablityCollection[0].StartTime.GetValueOrDefault()),
				    dateOnly.Date.Add(overtimeAvailablityCollection[0].EndTime.GetValueOrDefault()), timeZoneInfo);



		    return _adjustOvertimeLengthBasedOnAvailability.AdjustOvertimeDuration(overtimePeriod, overtimeLayerLengthPeriod, scheduleDay);
	    }
    }
}
