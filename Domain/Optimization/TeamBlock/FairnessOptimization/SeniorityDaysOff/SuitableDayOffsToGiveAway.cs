using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface ISuitableDayOffsToGiveAway
    {
		IList<DateOnly> DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints);
    }

    public class SuitableDayOffsToGiveAway : ISuitableDayOffsToGiveAway
    {
        public IList<DateOnly> DetectMostValuableSpot(IList<DateOnly> dayCollection, IDictionary<DayOfWeek, int> weekDayPoints)
        {
	        var valuableDayOffsToGiveAway = new List<DateOnly>();
            foreach (var lowestWeekDayPoint in weekDayPoints.OrderBy(s => s.Value))
            {
                foreach (var dateOnly in dayCollection)
                {
	                if (dateOnly.DayOfWeek == lowestWeekDayPoint.Key)
		                valuableDayOffsToGiveAway.Add(dateOnly);
                }
            }
			return valuableDayOffsToGiveAway;
        }
    }
}