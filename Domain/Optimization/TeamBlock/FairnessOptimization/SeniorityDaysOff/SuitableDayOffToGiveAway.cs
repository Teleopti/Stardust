using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ISuitableDayOffToGiveAway
    {
        DateOnly DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class SuitableDayOffToGiveAway : ISuitableDayOffToGiveAway
    {
        public DateOnly DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints)
        {
            foreach (var lowestWeekDayPoint in weekDayPoints.OrderBy(s => s.Value))
            {
                foreach (var dateOnly in dayCollection)
                {
                    if (dateOnly.DayOfWeek == lowestWeekDayPoint.Key)
                        return dateOnly;
                }
            }
            return DateOnly.MinValue;
        }
    }
}