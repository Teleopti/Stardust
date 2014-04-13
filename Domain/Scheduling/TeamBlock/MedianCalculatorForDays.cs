using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IMedianCalculatorForDays
    {
		Dictionary<DateTime, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> days, double resolution);
    }

    public class MedianCalculatorForDays : IMedianCalculatorForDays
    {
        private readonly IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;

        public MedianCalculatorForDays(IMedianCalculatorForSkillInterval medianCalculatorForSkillInterval)
        {
            _medianCalculatorForSkillInterval = medianCalculatorForSkillInterval;
        }

		public Dictionary<DateTime, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> days, double resolution)
        {
			var result = new Dictionary<DateTime, ISkillIntervalData>();
	        if (!days.Any())
		        return result;

			var temp = new Dictionary<DateTime, IList<ISkillIntervalData>>();
            var baseDate =new DateOnly(days.Keys.First().Date);

            foreach (var intervalData in days.SelectMany(day => day.Value))
            {
                if (!temp.ContainsKey(intervalData.Key))
                {
                    temp.Add(intervalData.Key, new List<ISkillIntervalData> { intervalData.Value });
                }
                else
                {
                    temp[intervalData.Key].Add(intervalData.Value);
                }
            }
            
            foreach (var interval in temp)
            {
                ISkillIntervalData skillIntervalData = _medianCalculatorForSkillInterval.CalculateMedian(interval.Key,
                                                                                                         interval.Value,
                                                                                                         resolution,
                                                                                                         baseDate);
                if(skillIntervalData != null )
                    result.Add(interval.Key, skillIntervalData);
            }

            return result;
        }
    }
}