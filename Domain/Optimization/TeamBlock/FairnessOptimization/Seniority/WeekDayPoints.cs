using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.Seniority
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
                    {DayOfWeek.Monday, 7},
                    {DayOfWeek.Tuesday, 6},
                    {DayOfWeek.Wednesday, 5},
                    {DayOfWeek.Thursday, 4},
                    {DayOfWeek.Friday, 3},
                    {DayOfWeek.Saturday, 2},
                    {DayOfWeek.Sunday, 1}
                };
        }
    }
}
