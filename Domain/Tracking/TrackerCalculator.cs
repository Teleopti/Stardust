using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Tracking
{
    public class TrackerCalculator : ITrackerCalculator
    {
        public TimeSpan CalculateTotalTimeOnScheduleDays(IPayload payload, IList<IScheduleDay> scheduleDays)
        {
            double totalMinutes = 0;
            foreach (IScheduleDay day in scheduleDays)
            {
                totalMinutes += calculateTotalTimeOnVisualLayerCollection(payload, day.ProjectionService().CreateProjection());
            }
            return TimeSpan.FromMinutes(totalMinutes);
        }

        public TimeSpan CalculateNumberOfDaysOnScheduleDays(IPayload payload, IList<IScheduleDay> scheduleDays)
        {
            var cntDays = 0;
            foreach (IScheduleDay day in scheduleDays)
            {
                var significantPart = day.SignificantPart();
                if (significantPart != SchedulePartView.DayOff && significantPart != SchedulePartView.ContractDayOff)
                    cntDays += calculateNumberOfDaysOnVisualLayerCollection(payload, day.ProjectionService().CreateProjection());
            }
            return TimeSpan.FromDays(cntDays);
        }


        private static double calculateTotalTimeOnVisualLayerCollection(IPayload payload, IVisualLayerCollection collection)
        {
            return collection.FilterLayers(payload).ContractTime().TotalMinutes;
        }

        private static int calculateNumberOfDaysOnVisualLayerCollection(IPayload payload, IVisualLayerCollection collection)
        {
            //Maybe change to use CalculateIfCountsAsDay when we know how the OneSpecificLayerSpecification.IsSatisfiedBy should work
            // if this in some cases should be more than one, then we can not use the specifikation
            IVisualLayerCollection filtered = collection.FilterLayers(payload);
            return filtered.Any() ? 1 : 0;
        }
    }
}
