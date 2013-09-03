using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalData
	{
		double ForecastedDemand { get; }
		DateTimePeriod Period { get; }
		double CurrentHeads { get; }
		double? MaximumHeads { get; }
        double? MinimumHeads { get; }
		double CurrentDemand { get; }
        double MinMaxBoostFactor { get; set; }
		TimeSpan Resolution();
	}

	public class SkillIntervalData : ISkillIntervalData
	{
		public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, double? minimumHeads, double? maximumHeads)
		{
			Period = period;
			ForecastedDemand = forecastedDemand;
			CurrentDemand = currentDemand;
			CurrentHeads = currentHeads;
			MinimumHeads = minimumHeads.HasValue && minimumHeads.Value < 0.001 ? null : minimumHeads;
			MaximumHeads = maximumHeads.HasValue && maximumHeads.Value < 0.001 ? null : maximumHeads;
            MinMaxBoostFactor = calculateMinMaxBoostFactor();
		}

	    private double calculateMinMaxBoostFactor()
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

	    public double ForecastedDemand { get; private set; }

		public DateTimePeriod Period { get; private set; }

		public double CurrentHeads { get; private set; }

        public double? MaximumHeads { get; private set; }

        public double? MinimumHeads { get; private set; }

		public double CurrentDemand { get; private set; }

        public double MinMaxBoostFactor { get; set; }

		public TimeSpan Resolution()
		{
			return Period.EndDateTime.Subtract(Period.StartDateTime);
		}
	}
}