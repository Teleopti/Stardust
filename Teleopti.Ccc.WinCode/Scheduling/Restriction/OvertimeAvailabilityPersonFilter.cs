﻿using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WinCode.Scheduling.Restriction
{
    public class OvertimeAvailabilityPersonFilter
    {
        public IList<IPerson> GetFilterdPerson(IList<IScheduleDay> scheduleDaysList, TimeSpan filterStartTime, TimeSpan filterEndTime)
        {
            var personList = new List<IPerson>();
            var overnightFilter = isPeriodOvernight(filterEndTime);
            foreach (var scheduleDay in scheduleDaysList)
            {
                var overtimeAvailability =
                    scheduleDay.PersistableScheduleDataCollection().OfType<IOvertimeAvailability>().FirstOrDefault();
                if (overtimeAvailability != null)
                {
                    var startTime = overtimeAvailability.StartTime.GetValueOrDefault();
                    var endTime = overtimeAvailability.EndTime.GetValueOrDefault();
                    var overnightShift = isPeriodOvernight(endTime);
                    if ((overnightShift && overnightFilter) || (!overnightShift && !overnightFilter))
                    {
                        if (startTime <= filterStartTime && endTime >= filterEndTime)
                            personList.Add(overtimeAvailability.Person);
                    }
                   
                }
            }
            return personList;
        }

        private bool isPeriodOvernight(TimeSpan filterEndTime)
        {
            return filterEndTime.Days > 0;
        }

    }
}