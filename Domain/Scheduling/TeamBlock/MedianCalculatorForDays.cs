using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IMedianCalculatorForDays
    {
        Dictionary<TimeSpan, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> days, double resolution);
    }

    public class MedianCalculatorForDays : IMedianCalculatorForDays
    {
        private readonly IIntervalDataCalculator _intervalDataCalculator;

        public MedianCalculatorForDays()
        {
            _intervalDataCalculator = new IntervalDataMedianCalculator();
        }

        public Dictionary<TimeSpan, ISkillIntervalData> CalculateMedian(Dictionary<DateOnly, Dictionary<TimeSpan, ISkillIntervalData>> days, double resolution)
        {
            var result = new Dictionary<TimeSpan, ISkillIntervalData>();
            var temp = new Dictionary<TimeSpan, IList<ISkillIntervalData>>();
            var baseDate = DateTime.SpecifyKind(SkillDayTemplate.BaseDate, DateTimeKind.Utc);

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
                var forecastedDemands = new List<double>();
                var currentDemands = new List<double>();
                forecastedDemands.AddRange(from item in interval.Value
                                           select item.ForecastedDemand);
                currentDemands.AddRange(from item in interval.Value
                                        select item.ForecastedDemand);
                if (forecastedDemands.Count == 0) continue;
                var calculatedFDemand = _intervalDataCalculator.Calculate(forecastedDemands);
                var calculatedCDemand = _intervalDataCalculator.Calculate(currentDemands);
                var startTime = baseDate.Date.Add(interval.Key);
                var endTime = startTime.AddMinutes(resolution);
                ISkillIntervalData skillIntervalData = new SkillIntervalData(
                    new DateTimePeriod(startTime, endTime), calculatedFDemand, calculatedCDemand, 0, 0, 0);
                result.Add(interval.Key, skillIntervalData);
            }

            return result;
        }
    }
}