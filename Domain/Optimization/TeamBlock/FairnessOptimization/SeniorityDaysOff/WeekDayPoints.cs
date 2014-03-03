using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IWeekDayPoints
    {
        IDictionary<DayOfWeek, int> GetWeekDaysPoints();
    }

    public class WeekDayPoints : IWeekDayPoints
    {
        private Dictionary<DayOfWeek, int> _weekDayProprity;

        public IDictionary<DayOfWeek, int> GetWeekDaysPoints()
        {
            populateReferencePoints();
            return _weekDayProprity;
        }

        private void populateReferencePoints()
        {
            _weekDayProprity = new Dictionary<DayOfWeek, int>
                {
                    {DayOfWeek.Monday, 1},
                    {DayOfWeek.Tuesday, 2},
                    {DayOfWeek.Wednesday, 3},
                    {DayOfWeek.Thursday, 4},
                    {DayOfWeek.Friday, 5},
                    {DayOfWeek.Saturday, 6},
                    {DayOfWeek.Sunday, 7}
                };
        }
    }
}
