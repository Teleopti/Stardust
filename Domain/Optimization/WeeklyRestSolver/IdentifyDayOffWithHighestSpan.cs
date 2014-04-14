using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public class IdentifyDayOffWithHighestSpan
    {
        public DateOnly GetHighProbableDayOffPosition(IDictionary<DateOnly, TimeSpan> possiblePositionsToFix)
        {
            var higestSpan = TimeSpan.MinValue;
            var resultDate = new DateOnly();
            foreach (var day in possiblePositionsToFix)
            {
                if (day.Value > higestSpan)
                {
                    higestSpan = day.Value;
                    resultDate = day.Key;
                }
            }
            return resultDate;
        }
    }
}
