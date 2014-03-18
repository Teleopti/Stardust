using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class AdjustOvertimeLengthBasedOnAvailability
    {
        public TimeSpan AdjustOvertimeDuration(DateTimePeriod overtimeAvailabilityPeriod, TimeSpan overtimeLayerLength, DateTime shiftEndTime)
        {
            if (shiftEndTime < overtimeAvailabilityPeriod.StartDateTime) return TimeSpan.Zero;
            var possibleAdjustDifference = (overtimeAvailabilityPeriod.EndDateTime - shiftEndTime).TotalMinutes;
            if (possibleAdjustDifference <= 0) return TimeSpan.Zero;
            if (possibleAdjustDifference <= overtimeLayerLength.TotalMinutes)
                return TimeSpan.FromMinutes(possibleAdjustDifference);
            return TimeSpan.FromMinutes(overtimeLayerLength.TotalMinutes);
        }

    }
}