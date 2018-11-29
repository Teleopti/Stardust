using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.DayOff
{
	public interface ISplitSchedulePeriodToWeekPeriod
	{
		IList<DateOnlyPeriod> Split(DateOnlyPeriod dateTimePeriod, DayOfWeek workWeekStartsAt);
	}

	public class SplitSchedulePeriodToWeekPeriod : ISplitSchedulePeriodToWeekPeriod
	{
        public IList<DateOnlyPeriod> Split(DateOnlyPeriod dateTimePeriod, DayOfWeek workWeekStartsAt)
        {
            var resultList = new List<DateOnlyPeriod>();

			var firstWeekStartDate = DateHelper.GetFirstDateInWeek(dateTimePeriod.StartDate, workWeekStartsAt);
	        var lastWeekEndDate = DateHelper.GetLastDateInWeek(dateTimePeriod.EndDate, workWeekStartsAt);
			while (firstWeekStartDate <= lastWeekEndDate)
	        {
		        resultList.Add(new DateOnlyPeriod(firstWeekStartDate, firstWeekStartDate.AddDays(6)));
		        firstWeekStartDate = firstWeekStartDate.AddDays(7);
	        }

            return resultList;
        }
       
    }
}