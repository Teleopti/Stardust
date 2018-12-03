using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Overtime
{
    public class AdjustOvertimeLengthBasedOnAvailability
    {
        public DateTimePeriod? AdjustOvertimeDuration(DateTimePeriod overtimeAvailabilityPeriod, DateTimePeriod overtimeLayerLengthPeriod, IScheduleDay scheduleDay)
        {
			var shiftPeriod = scheduleDay.ProjectionService().CreateProjection().Period().GetValueOrDefault();
			var shiftStart = shiftPeriod.StartDateTime;
			var shiftEnd = shiftPeriod.EndDateTime;

			var intersection = overtimeAvailabilityPeriod.Intersection(overtimeLayerLengthPeriod);
	        if (intersection == null)
				return null;  
			
	        if (!overtimeAvailabilityPeriod.Contains(overtimeLayerLengthPeriod) && overtimeLayerLengthPeriod.StartDateTime < shiftStart)
	        {
		        if (intersection.Value.EndDateTime < overtimeLayerLengthPeriod.EndDateTime)
					return null;

			    return  new DateTimePeriod(intersection.Value.StartDateTime, overtimeLayerLengthPeriod.EndDateTime);     
	        }

			if (!overtimeAvailabilityPeriod.Contains(overtimeLayerLengthPeriod) && overtimeLayerLengthPeriod.EndDateTime > shiftEnd)
			{
				if (overtimeAvailabilityPeriod.ElapsedTime() < overtimeLayerLengthPeriod.ElapsedTime())
					return null;

		        if (intersection.Value.StartDateTime > shiftEnd)
					return null;

				return new DateTimePeriod(overtimeLayerLengthPeriod.StartDateTime, intersection.Value.EndDateTime);
	        }

			return overtimeLayerLengthPeriod;
        }
    }
}