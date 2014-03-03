using System.Collections.Generic;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
    public class SplitSchedulePeriodToWeekPeriod
    {
        public IList<DateOnlyPeriod> Split(DateOnlyPeriod dateTimePeriod)
        {
            var resultList = new List<DateOnlyPeriod>();
            var startDateTime = dateTimePeriod.StartDate;
            
            while (startDateTime <= dateTimePeriod.EndDate)
            {
                var endDateTime = startDateTime;
                for (int i = 0; i < 6; i++)
                {
                    if (!dateTimePeriod.Contains(endDateTime.AddDays(1))) break;
                    endDateTime = endDateTime.AddDays(1);
                }
                resultList.Add(new DateOnlyPeriod(startDateTime, endDateTime));
                startDateTime = endDateTime.AddDays(1);
            }
            return resultList;
        }
       
    }
}