using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Sdk.ServiceBus.Forecast
{
    public interface IWorkloadDayOpenHoursDictionary
    {
        void Add(DateOnly dateOnly, TimePeriod openHours);
        TimePeriod Get(DateOnly dateOnly);
    }

    public class WorkloadDayOpenHoursDictionary : IWorkloadDayOpenHoursDictionary
    {
        private readonly IDictionary<DateOnly, TimePeriod> _workloadDayOpenHours =
            new Dictionary<DateOnly, TimePeriod>();

        public void Add(DateOnly dateOnly, TimePeriod openHours)
        {
            TimePeriod existingOpenHours;
            if (_workloadDayOpenHours.TryGetValue(dateOnly, out existingOpenHours))
            {
                TimeSpan mergedStartTime;
                TimeSpan mergedEndTime;
                if (openHours.StartTime.Subtract(existingOpenHours.StartTime) < TimeSpan.Zero)
                {
                    mergedStartTime = openHours.StartTime;
                    mergedEndTime = existingOpenHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
                if (openHours.EndTime.Subtract(existingOpenHours.EndTime) > TimeSpan.Zero)
                {
                    mergedStartTime = existingOpenHours.StartTime;
                    mergedEndTime = openHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
            }
            else
                _workloadDayOpenHours.Add(dateOnly, openHours);
        }

        public TimePeriod Get(DateOnly dateOnly)
        {
            TimePeriod openHours;
            _workloadDayOpenHours.TryGetValue(dateOnly, out openHours);
            return openHours;
        }
    }
}