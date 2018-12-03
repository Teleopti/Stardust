using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface ISkillIntervalDataOpenHour
    {
		TimePeriod? GetOpenHours(IEnumerable<ISkillIntervalData> skillIntervalDataList, DateOnly skillDayDate);
    }

    public class SkillIntervalDataOpenHour : ISkillIntervalDataOpenHour
    {
        
        public TimePeriod? GetOpenHours(IEnumerable<ISkillIntervalData> skillIntervalDataList, DateOnly skillDayDate)
        {
	        if (!skillIntervalDataList.Any())
		        return null;

            var minDateTime = (from o in skillIntervalDataList
                           select o.Period.StartDateTime).Min();
            var maxDateTime = (from o in skillIntervalDataList
                           select o.Period.EndDateTime).Max();

	        var minTime = minDateTime.TimeOfDay;
	        int minDayOffset = (int)new DateOnly(minDateTime).Subtract(skillDayDate).TotalDays;
	        minTime = minTime.Add(TimeSpan.FromDays(minDayOffset));

	        var maxTime = maxDateTime.TimeOfDay;
	        int maxDayOffset = (int)new DateOnly(maxDateTime).Subtract(skillDayDate).TotalDays;
	        maxTime = maxTime.Add(TimeSpan.FromDays(maxDayOffset));
    
            return new TimePeriod(minTime, maxTime);
        }
    }
}