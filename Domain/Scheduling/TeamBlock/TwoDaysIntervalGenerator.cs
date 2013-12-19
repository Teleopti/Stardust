using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    
    public interface ITwoDaysIntervalGenerator
    {
	    Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(
		    IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
    }

    public class TwoDaysIntervalGenerator : ITwoDaysIntervalGenerator 
    {
	    public Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> GenerateTwoDaysInterval(
		    IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData)
	    {
		    var twoDayIntervalsForAllDays = new Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>>();

		    var firstDate = dayIntervalData.Keys.Min();
		    for (int i = 0; i < dayIntervalData.Count - 1; i++)
		    {
			    var dateOnly = firstDate.AddDays(i);
			    var timeSpanDic = new Dictionary<TimeSpan, ISkillIntervalData>();

			    var sourceList = dayIntervalData[dateOnly];
			    foreach (var skillIntervalData in sourceList)
			    {
				    var keyTimeSpan = toLocalTimeKey(skillIntervalData.Period.StartDateTime, dateOnly);
				    timeSpanDic.Add(keyTimeSpan, skillIntervalData);
			    }

				sourceList = dayIntervalData[dateOnly.AddDays(1)];
				foreach (var skillIntervalData in sourceList)
				{
					var keyTimeSpan = toLocalTimeKey(skillIntervalData.Period.StartDateTime, dateOnly);
					timeSpanDic.Add(keyTimeSpan, skillIntervalData);
				}

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
