using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    
    public interface ITwoDaysIntervalGenerator
    {
	    Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
    }

    public class TwoDaysIntervalGenerator : ITwoDaysIntervalGenerator 
    {
	    public Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(
            IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData)
        {
            var twoDayIntervalsForAllDays = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();
            
            foreach(var dateOnly in dayIntervalData.Keys )
            {
                var timeSpanDic = new Dictionary<TimeSpan, ISkillIntervalData>();
                if (dayIntervalData.ContainsKey(dateOnly))
                {
                    IList<ISkillIntervalData> sourceList = dayIntervalData[dateOnly];
                    foreach (var skillIntervalData in sourceList)
                    {
                        var keyTimeSpan = toLocalTimeKey(skillIntervalData.Period.StartDateTime, dateOnly);
                        timeSpanDic.Add(keyTimeSpan, skillIntervalData);
                    }

                    if (dayIntervalData.ContainsKey(dateOnly.AddDays(1)))
                    {
                        sourceList = dayIntervalData[dateOnly.AddDays(1)];
                        foreach (var skillIntervalData in sourceList)
                        {
                            var keyTimeSpan = toLocalTimeKey(skillIntervalData.Period.StartDateTime, dateOnly);
                            timeSpanDic.Add(keyTimeSpan, skillIntervalData);
                        }
                    }
                }
                if (timeSpanDic.Keys.Count > 0)
                    twoDayIntervalsForAllDays.Add(dateOnly, timeSpanDic);
            }

            return twoDayIntervalsForAllDays;
        }

	    private TimeSpan toLocalTimeKey(DateTime dateTime, DateOnly baseDate)
		{
			var dateOffset = dateTime.Date.Subtract(baseDate);
			return dateTime.TimeOfDay.Add(dateOffset);
		}
    }
}
