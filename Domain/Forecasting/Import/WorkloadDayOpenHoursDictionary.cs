using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Import
{
    public interface IWorkloadDayOpenHoursContainer
    {
        void AddOpenHour(DateOnly dateOnly, TimePeriod openHours);
        TimePeriod GetOpenHour(DateOnly dateOnly);
    }

    public class WorkloadDayOpenHoursContainer : IWorkloadDayOpenHoursContainer
    {
        private readonly IDictionary<DateOnly, TimePeriod> _workloadDayOpenHours =
            new Dictionary<DateOnly, TimePeriod>();

        public void AddOpenHour(DateOnly dateOnly, TimePeriod openHours)
        {
            TimePeriod existingOpenHours;
            if (_workloadDayOpenHours.TryGetValue(dateOnly, out existingOpenHours))
            {
                TimeSpan mergedStartTime;
                TimeSpan mergedEndTime;
                if (openHours.StartTime < existingOpenHours.StartTime)
                {
                    mergedStartTime = openHours.StartTime;
                    mergedEndTime = existingOpenHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
                if (openHours.EndTime > existingOpenHours.EndTime)
                {
                    mergedStartTime = existingOpenHours.StartTime;
                    mergedEndTime = openHours.EndTime;
                    _workloadDayOpenHours[dateOnly] = new TimePeriod(mergedStartTime, mergedEndTime);
                }
            }
            else
                _workloadDayOpenHours.Add(dateOnly, openHours);
        }

        public TimePeriod GetOpenHour(DateOnly dateOnly)
        {
            TimePeriod openHours;
            _workloadDayOpenHours.TryGetValue(dateOnly, out openHours);
            return openHours;
        }
    }
}