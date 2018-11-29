using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Optimization.WeeklyRestSolver
{
    public interface IExtractDayOffFromGivenWeek
    {
        IList<DateOnly> GetDaysOff(IEnumerable<IScheduleDay> scheduleDayList);
    }
    public class ExtractDayOffFromGivenWeek : IExtractDayOffFromGivenWeek
    {
        public IList< DateOnly> GetDaysOff(IEnumerable<IScheduleDay> scheduleDayList)
        {
	        var daysOff = new List<DateOnly>();
            foreach (var scheduleDay in scheduleDayList)
            {
                var significantPart = scheduleDay.SignificantPart();
                if (significantPart == SchedulePartView.DayOff)
                {
                    daysOff.Add(scheduleDay.DateOnlyAsPeriod.DateOnly);
                }
                    
            }
            return daysOff;
        }
    }
}