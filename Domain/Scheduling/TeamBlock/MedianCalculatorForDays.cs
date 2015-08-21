using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IMedianCalculatorForDays
    {
		Dictionary<DateTime, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> days, double resolution, DateOnly returnListDateOnly);
    }

    public class MedianCalculatorForDays : IMedianCalculatorForDays
    {
        private readonly IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;

        public MedianCalculatorForDays(IMedianCalculatorForSkillInterval medianCalculatorForSkillInterval)
        {
            _medianCalculatorForSkillInterval = medianCalculatorForSkillInterval;
        }

		public Dictionary<DateTime, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> days, double resolution, DateOnly returnListDateOnly)
        {
			var result = new Dictionary<DateTime, ISkillIntervalData>();
	        if (!days.Any())
		        return result;

			var temp = new Dictionary<TimeSpan, IList<ISkillIntervalData>>();
			foreach (var dateOnlyList in days)
			{
				var baseDate = dateOnlyList.Key;
				foreach (var interval in dateOnlyList.Value)
				{
					var timeSpanKey = interval.Key.TimeOfDay;
					if (new DateOnly(interval.Key).Equals(baseDate.AddDays(1)))
						timeSpanKey = timeSpanKey.Add(TimeSpan.FromDays(1));

					IList<ISkillIntervalData> value;
					if (!temp.TryGetValue(timeSpanKey, out value))
					{
						value = new List<ISkillIntervalData>();
						temp.Add(timeSpanKey, value);
					}

					value.Add(interval.Value);
				}
			}

            foreach (var interval in temp)
            {
                ISkillIntervalData skillIntervalData = _medianCalculatorForSkillInterval.CalculateMedian(interval.Key,
                                                                                                         interval.Value,
                                                                                                         resolution,
																										 returnListDateOnly);
                if(skillIntervalData != null )
                    result.Add(skillIntervalData.Period.StartDateTime, skillIntervalData);
            }

            return result;
        }
    }
}