using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.Restriction;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public interface IOvertimeAvailabilityCreator
    {
        IOvertimeAvailability Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime);
        bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError);
    }

    public class  OvertimeAvailabilityCreator : IOvertimeAvailabilityCreator
    {
        public IOvertimeAvailability Create(IScheduleDay scheduleDay, TimeSpan? startTime, TimeSpan? endTime)
        {
            if (scheduleDay == null) throw new ArgumentNullException("scheduleDay");

            bool startTimeError;
            bool endTimeError;
            if (!CanCreate(startTime, endTime, out startTimeError, out endTimeError)) return null;


            var overtimeAvailability = new OvertimeAvailability(scheduleDay.Person,
                                                                scheduleDay.DateOnlyAsPeriod.DateOnly,startTime ,endTime);

            return overtimeAvailability;
        }

	    public bool CanCreate(TimeSpan? startTime, TimeSpan? endTime, out bool startTimeError, out bool endTimeError)
        {
            if (startTime == null && endTime == null)
            {
                startTimeError = true;
                endTimeError = true;
                return false;	
            }
				
            if (startTime != null && endTime != null)
            {
                if (startTime.Value >= endTime.Value)
                {
                    startTimeError = true;
                    endTimeError = false;
                    return false;
                }
            }

            if (startTime == null)
            {
                startTimeError = true;
                endTimeError = false;
                return false;
            }

            if (endTime == null)
            {
                startTimeError = false;
                endTimeError = true;
                return false;
            }

            startTimeError = false;
            endTimeError = false;
            return true;
		
        }
    }
}