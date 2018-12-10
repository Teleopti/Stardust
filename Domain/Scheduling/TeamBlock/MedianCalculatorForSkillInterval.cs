using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;

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
			var minMaxBoostFactorForStandardDeviation = 0d;

            foreach (var intervalData in skillIntervalDataList)
            {
				forecastedDemands.Add(intervalData.ForecastedDemand);
				currentDemands.Add(intervalData.CurrentDemand);
				minMaxBoostFactor += intervalData.MinMaxBoostFactor;
	            minMaxBoostFactorForStandardDeviation += intervalData.MinMaxBoostFactorForStandardDeviation;
            }

            if (forecastedDemands.Count == 0) return null ;
            var calculatedFDemand = _intervalDataCalculator.Calculate(forecastedDemands);
            var calculatedCDemand = _intervalDataCalculator.Calculate(currentDemands);

            var startTime = DateTime.SpecifyKind(baseDate.Date.Add(key),DateTimeKind.Utc);
			var endTime = startTime.AddMinutes(resolution);
            ISkillIntervalData skillIntervalData = new SkillIntervalData(
                new DateTimePeriod(startTime, endTime), calculatedFDemand, calculatedCDemand, 0 , null ,null);
			skillIntervalData.MinMaxBoostFactor = minMaxBoostFactor;
			skillIntervalData.MinMaxBoostFactorForStandardDeviation = minMaxBoostFactorForStandardDeviation;
            return skillIntervalData;
        }
    }
}