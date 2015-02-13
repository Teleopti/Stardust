using System;
using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Optimization.TeamBlock.FairnessOptimization.SeniorityDaysOff
{
    public interface IWeekDayPoints
    {
		IDictionary<DayOfWeek, int> GetWeekDaysPoints(ISeniorityWorkDayRanks seniorityWorkDayRanks);
    }

    public class WeekDayPoints : IWeekDayPoints
    {
        private Dictionary<DayOfWeek, int> _weekDayProprity;

        public IDictionary<DayOfWeek, int> GetWeekDaysPoints(ISeniorityWorkDayRanks seniorityWorkDayRanks)
        {
            populateReferencePoints(seniorityWorkDayRanks);
            return _weekDayProprity;
        }

		private void populateReferencePoints(ISeniorityWorkDayRanks seniorityWorkDayRanks)
        {
            _weekDayProprity = new Dictionary<DayOfWeek, int>
                {
                    {DayOfWeek.Monday, seniorityWorkDayRanks.Monday},
                    {DayOfWeek.Tuesday, seniorityWorkDayRanks.Tuesday},
                    {DayOfWeek.Wednesday, seniorityWorkDayRanks.Wednesday},
                    {DayOfWeek.Thursday, seniorityWorkDayRanks.Thursday},
                    {DayOfWeek.Friday, seniorityWorkDayRanks.Friday},
                    {DayOfWeek.Saturday, seniorityWorkDayRanks.Saturday},
                    {DayOfWeek.Sunday, seniorityWorkDayRanks.Sunday}
                };
        }
    }
}
