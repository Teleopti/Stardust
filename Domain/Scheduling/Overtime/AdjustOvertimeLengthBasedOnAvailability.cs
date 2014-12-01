using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class AdjustOvertimeLengthBasedOnAvailability
    {
        public DateTimePeriod? AdjustOvertimeDuration(DateTimePeriod overtimeAvailabilityPeriod, DateTimePeriod overtimeLayerLengthPeriod, DateTime shiftEndTime)
        {
	        if (shiftEndTime < overtimeAvailabilityPeriod.StartDateTime) return null;
            var possibleAdjustDifference = (overtimeAvailabilityPeriod.EndDateTime - shiftEndTime).TotalMinutes;
	        if (possibleAdjustDifference <= 0) return null;
	        if (possibleAdjustDifference <= overtimeLayerLengthPeriod.ElapsedTime().TotalMinutes)
				return new DateTimePeriod(overtimeLayerLengthPeriod.StartDateTime, overtimeLayerLengthPeriod.StartDateTime.AddMinutes(possibleAdjustDifference));
              
	        return overtimeLayerLengthPeriod;
        }
    }
}