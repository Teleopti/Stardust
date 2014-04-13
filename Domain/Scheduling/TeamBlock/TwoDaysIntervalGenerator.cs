using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    
    public interface ITwoDaysIntervalGenerator
    {
	    Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> GenerateTwoDaysInterval(IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData);
    }

    public class TwoDaysIntervalGenerator : ITwoDaysIntervalGenerator 
    {
		public Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> GenerateTwoDaysInterval(
            IDictionary<DateOnly, IList<ISkillIntervalData>> dayIntervalData)
        {
			var twoDayIntervalsForAllDays = new Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>>();

		    var lastDate = dayIntervalData.Keys.Max();
            foreach(var dateOnly in dayIntervalData.Keys )
            {
				//skip last date
				if(dateOnly == lastDate)
					continue;

                var dic = new Dictionary<DateTime, ISkillIntervalData>();
                if (dayIntervalData.ContainsKey(dateOnly))
                {
                    IList<ISkillIntervalData> sourceList = dayIntervalData[dateOnly];
                    foreach (var skillIntervalData in sourceList)
                    {
                        dic.Add(skillIntervalData.Period.StartDateTime, skillIntervalData);
                    }
                    if (sourceList.Count == 0) continue;
                    if (dayIntervalData.ContainsKey(dateOnly.AddDays(1)))
                    {
                        sourceList = dayIntervalData[dateOnly.AddDays(1)];
                        foreach (var skillIntervalData in sourceList)
                        {
							dic.Add(skillIntervalData.Period.StartDateTime, skillIntervalData);
                        }
                    }
                }
                if (dic.Keys.Count > 0)
                    twoDayIntervalsForAllDays.Add(dateOnly, dic);
            }

            return twoDayIntervalsForAllDays;
        }
    }
}
