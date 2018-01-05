﻿using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public class MedianCalculatorForDays
    {
		private readonly IMedianCalculatorForSkillInterval _medianCalculatorForSkillInterval;

		public MedianCalculatorForDays(IMedianCalculatorForSkillInterval medianCalculatorForSkillInterval)
		{
			_medianCalculatorForSkillInterval = medianCalculatorForSkillInterval;
		}

		public Dictionary<DateTime, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<DateTime, ISkillIntervalData>> days, double resolution, DateOnly returnListDateOnly)
		{
			var result = new Dictionary<DateTime, ISkillIntervalData>();
			var temp = new Dictionary<TimeSpan, IList<ISkillIntervalData>>();
			foreach (var dateOnlyList in days)
			{
				var baseDate = dateOnlyList.Key;
				foreach (var interval in dateOnlyList.Value)
				{
					var timeSpanKey = interval.Key.TimeOfDay;
					var intervalDateOnly = new DateOnly(interval.Key);

					if (intervalDateOnly == returnListDateOnly.AddDays(-1))
						timeSpanKey = timeSpanKey.Add(TimeSpan.FromDays(-1));

					if (intervalDateOnly == baseDate.AddDays(1))
						timeSpanKey = timeSpanKey.Add(TimeSpan.FromDays(1));

					if (!temp.TryGetValue(timeSpanKey, out var value))
					{
						value = new List<ISkillIntervalData>();
						temp.Add(timeSpanKey, value);
					}

					value.Add(interval.Value);
				}
			}

			foreach (var interval in temp)
			{
				var skillIntervalData = _medianCalculatorForSkillInterval.CalculateMedian(interval.Key,
					interval.Value,
					resolution,
					returnListDateOnly);
				if (skillIntervalData != null)
					result.Add(skillIntervalData.Period.StartDateTime, skillIntervalData);
			}

			return result;
		}
    }
}