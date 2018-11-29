using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

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
	        var count = dayIntervalData.Count;
            foreach(var dateOnly in dayIntervalData.Keys )
            {
				//skip last date
				if(dateOnly == lastDate && count > 1)
					continue;

                var dic = new Dictionary<DateTime, ISkillIntervalData>();
	            IList<ISkillIntervalData> sourceList;
                if (dayIntervalData.TryGetValue(dateOnly, out sourceList))
                {
                    foreach (var skillIntervalData in sourceList)
                    {
						if (!dic.ContainsKey(skillIntervalData.Period.StartDateTime))
							dic.Add(skillIntervalData.Period.StartDateTime, skillIntervalData);
                    }
                    if (sourceList.Count == 0) continue;
                    if (dayIntervalData.TryGetValue(dateOnly.AddDays(1), out sourceList))
                    {
                        foreach (var skillIntervalData in sourceList)
                        {
							if (!dic.ContainsKey(skillIntervalData.Period.StartDateTime))
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
