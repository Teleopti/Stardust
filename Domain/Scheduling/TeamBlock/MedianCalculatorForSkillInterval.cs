using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IMedianCalculatorForSkillInterval
	{
		ISkillIntervalData CalculateMedian(TimeSpan key, IList<ISkillIntervalData> skillIntervalData, double resolution, DateOnly baseDate);
    }

    public class MedianCalculatorForSkillInterval : IMedianCalculatorForSkillInterval 
    {
        private readonly IIntervalDataCalculator _intervalDataCalculator;

        public MedianCalculatorForSkillInterval(IIntervalDataCalculator intervalDataCalculator)
        {
            _intervalDataCalculator = intervalDataCalculator;
        }

		public ISkillIntervalData CalculateMedian(TimeSpan key, IList<ISkillIntervalData> skillIntervalDataList, double resolution, DateOnly baseDate)
        {
            var forecastedDemands = new List<double>();
            var currentDemands = new List<double>();
			var minMaxBoostFactor = 0d;

            forecastedDemands.AddRange(from item in skillIntervalDataList
                                       select item.ForecastedDemand);
            currentDemands.AddRange(from item in skillIntervalDataList
                                    select item.CurrentDemand);
 
            foreach (var intervalData in skillIntervalDataList)
            {
				forecastedDemands.Add(intervalData.ForecastedDemand);
				currentDemands.Add(intervalData.CurrentDemand);
				minMaxBoostFactor += intervalData.MinMaxBoostFactor;
            }

            if (forecastedDemands.Count == 0) return null ;
            var calculatedFDemand = _intervalDataCalculator.Calculate(forecastedDemands);
            var calculatedCDemand = _intervalDataCalculator.Calculate(currentDemands);

            var startTime = DateTime.SpecifyKind(baseDate.Date.Add(key),DateTimeKind.Utc);
			var endTime = DateTime.SpecifyKind(startTime.AddMinutes(resolution), DateTimeKind.Utc);
            ISkillIntervalData skillIntervalData = new SkillIntervalData(
                new DateTimePeriod(startTime, endTime), calculatedFDemand, calculatedCDemand, 0 , null ,null);
			skillIntervalData.MinMaxBoostFactor = minMaxBoostFactor;
            return skillIntervalData;
        }
    }
}