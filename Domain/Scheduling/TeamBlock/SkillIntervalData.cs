using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalData
	{
		double ForecastedDemand { get; set; }
		DateTimePeriod Period { get; }
		double CurrentHeads { get; set; }
		double? MaximumHeads { get; set; }
        double? MinimumHeads { get; set; }
		double CurrentDemand { get; set; }
        double BoostedValue { get; set; }
		TimeSpan Resolution();
	}

	public class SkillIntervalData : ISkillIntervalData
	{
		private readonly DateTimePeriod _period;

        public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, double? minimumHeads, double? maximumHeads)
		{
			_period = period;
			ForecastedDemand = forecastedDemand;
			CurrentDemand = currentDemand;
			CurrentHeads = currentHeads;
			MinimumHeads = minimumHeads;
			MaximumHeads = maximumHeads;
            BoostedValue = calculateBoostedValue();
		}

	    private double calculateBoostedValue()
	    {
	        double minHeadValue = 0;
	        double maxHeadValue = 0;
            if (MinimumHeads.HasValue)
	        {
	            if (CurrentHeads < MinimumHeads.Value)
	                minHeadValue =  (MinimumHeads.Value - CurrentHeads);
	        }
            if (MaximumHeads.HasValue)
            {
                if (CurrentHeads >= MaximumHeads.Value)
                    maxHeadValue = MaximumHeads.Value - (CurrentHeads + 1);
            }
	        return minHeadValue + maxHeadValue;
	    }

	    public double ForecastedDemand { get; set; }

		public DateTimePeriod Period
		{
			get { return _period; }
		}

		public double CurrentHeads { get; set; }

        public double? MaximumHeads { get; set; }

        public double? MinimumHeads { get; set; }

		public double CurrentDemand { get; set; }

        public double BoostedValue { get; set; }

		public TimeSpan Resolution()
		{
			return Period.EndDateTime.Subtract(Period.StartDateTime);
		}
	}
}