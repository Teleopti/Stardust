using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ISuitableDayOffSpotDetector
    {
        DateOnly DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class SuitableDayOffSpotDetector : ISuitableDayOffSpotDetector
    {
        public DateOnly DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            foreach (var higestWeekDayPoint in weekDayPoints.OrderByDescending(s=>s.Value ))
            {
                foreach (var dateOnly in dayCollection)
                {
                    if (dateOnly.DayOfWeek == higestWeekDayPoint.Key)
                        return dateOnly;
                }
            }
            return DateOnly.MinValue;
        }
    }
}
