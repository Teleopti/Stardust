using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
    public interface IMedianCalculatorForSkillInterval
	{
		ISkillIntervalData CalculateMedian(DateTime key, IList<ISkillIntervalData> skillIntervalData, double resolution, DateOnly baseDate);
    }

    public class MedianCalculatorForSkillInterval : IMedianCalculatorForSkillInterval 
    {
        private readonly IIntervalDataCalculator _intervalDataCalculator;

        public MedianCalculatorForSkillInterval(IIntervalDataCalculator intervalDataCalculator)
        {
            _intervalDataCalculator = intervalDataCalculator;
        }

		public ISkillIntervalData CalculateMedian(DateTime key, IList<ISkillIntervalData> skillIntervalDataList, double resolution, DateOnly baseDate)
        {
            var forecastedDemands = new List<double>();
            var currentDemands = new List<double>();
            var currentHeads = new List<double>();
            var minimumStaffs = new List<double>();
            var maximumStaffs = new List<double>();

            forecastedDemands.AddRange(from item in skillIntervalDataList
                                       select item.ForecastedDemand);
            currentDemands.AddRange(from item in skillIntervalDataList
                                    select item.CurrentDemand);
            currentHeads.AddRange(from item in skillIntervalDataList 
                                  select item.CurrentHeads );
            foreach (var val in skillIntervalDataList)
            {
                if(val.MinimumHeads.HasValue )
                    minimumStaffs.Add(val.MinimumHeads.Value  );
            }
            foreach (var val in skillIntervalDataList)
            {
                if (val.MaximumHeads.HasValue)
                    maximumStaffs.Add(val.MaximumHeads .Value);
            }
            if (forecastedDemands.Count == 0) return null ;
            var calculatedFDemand = _intervalDataCalculator.Calculate(forecastedDemands);
            var calculatedCDemand = _intervalDataCalculator.Calculate(currentDemands);
            var currentHead = _intervalDataCalculator.Calculate(currentHeads );
            var minimumStaff  = _intervalDataCalculator.Calculate(minimumStaffs);
            var maximumStaff  = _intervalDataCalculator.Calculate(maximumStaffs);

            var startTime = DateTime.SpecifyKind(baseDate.Date.Add(key.TimeOfDay),DateTimeKind.Utc);
			var endTime = DateTime.SpecifyKind(startTime.AddMinutes(resolution), DateTimeKind.Utc);
            ISkillIntervalData skillIntervalData = new SkillIntervalData(
                new DateTimePeriod(startTime, endTime), calculatedFDemand, calculatedCDemand, currentHead , minimumStaff ,maximumStaff);
            return skillIntervalData;
        }
    }
}