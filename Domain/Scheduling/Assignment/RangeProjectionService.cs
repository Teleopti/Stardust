using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.Assignment
{
    public class RangeProjectionService : IRangeProjectionService
    {
        public IEnumerable<IVisualLayer> CreateProjection(IScheduleRange scheduleRange, DateTimePeriod period)
        {
            var timeZone = scheduleRange.Person.PermissionInformation.DefaultTimeZone();
            var dateOnlyPeriod = period.ToDateOnlyPeriod(timeZone);
            var listOfVisualLayers = new List<IVisualLayer>();
            foreach (DateOnly day in dateOnlyPeriod.DayCollection())
            {
                IScheduleDay scheduleDay = scheduleRange.ScheduledDay(day);
                if (scheduleDay.HasProjection())
                {
                    listOfVisualLayers.AddRange(scheduleDay.ProjectionService().CreateProjection().FilterLayers(period));
                }
            }
            return listOfVisualLayers;
        }
    }
}