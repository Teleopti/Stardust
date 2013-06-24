using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public interface IOvertimeAvailabilityPersonFilter
    {
        IList<IPerson> GetFilterdPerson(IList<IScheduleDay> scheduleDaysList, TimeSpan filterStartTime, TimeSpan filterEndTime);
    }
    public class OvertimeAvailabilityPersonFilter : IOvertimeAvailabilityPersonFilter
    {
        public IList<IPerson> GetFilterdPerson(IList<IScheduleDay> scheduleDaysList, TimeSpan filterStartTime, TimeSpan filterEndTime)
        {
            var personList = new List<IPerson>();
            foreach (var scheduleDay in scheduleDaysList)
            {
                var overtimeAvailability =
                    scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
                if (overtimeAvailability != null)
                {
                    var startTime = overtimeAvailability.StartTime.GetValueOrDefault();
                    var endTime = overtimeAvailability.EndTime.GetValueOrDefault();
                    if( startTime  <= filterStartTime  && endTime >=filterEndTime  )
                        personList.Add(overtimeAvailability.Person);
                }
            }
            return personList;
        }
    }
}