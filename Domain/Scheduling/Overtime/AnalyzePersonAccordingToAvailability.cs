using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public interface IAnalyzePersonAccordingToAvailability
    {
		IList<DateTimePeriod> AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo, IList<DateTimePeriod> overtimeLayerLengthPeriods);
    }

    public class AnalyzePersonAccordingToAvailability : IAnalyzePersonAccordingToAvailability
    {
        private readonly AdjustOvertimeLengthBasedOnAvailability _adjustOvertimeLengthBasedOnAvailability;

        public AnalyzePersonAccordingToAvailability(AdjustOvertimeLengthBasedOnAvailability adjustOvertimeLengthBasedOnAvailability)
        {
            _adjustOvertimeLengthBasedOnAvailability = adjustOvertimeLengthBasedOnAvailability;
        }

		public IList<DateTimePeriod> AdustOvertimeAvailability(IScheduleDay scheduleDay, DateOnly dateOnly, TimeZoneInfo timeZoneInfo, IList<DateTimePeriod> overtimeLayerLengthPeriods)
        {
			var adjustedList = new List<DateTimePeriod>();

            var ovrtimeCollectionForPerson = scheduleDay.OvertimeAvailablityCollection();
            if (ovrtimeCollectionForPerson.Count == 0) return adjustedList;
            var overtimePeriod = TimeZoneHelper.NewUtcDateTimePeriodFromLocalDateTime(dateOnly.Date.Add(ovrtimeCollectionForPerson[0].StartTime.GetValueOrDefault()),
                                                                dateOnly.Date.Add(ovrtimeCollectionForPerson[0].EndTime.GetValueOrDefault( )),timeZoneInfo);


			foreach (var overtimeLayerLengthPeriod in overtimeLayerLengthPeriods)
			{
				var adjustedPeriod = _adjustOvertimeLengthBasedOnAvailability.AdjustOvertimeDuration(overtimePeriod, overtimeLayerLengthPeriod, scheduleDay);
				if (adjustedPeriod.HasValue)
				{
					adjustedList.Add(adjustedPeriod.Value);
				}
			}

			return adjustedList;
        }
    }
}
