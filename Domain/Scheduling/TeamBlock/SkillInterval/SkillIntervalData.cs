using System;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock.SkillInterval
{
  public interface ISkillIntervalData : IWorkShiftPeriodData
	{
		DateTimePeriod Period { get; }
		double CurrentHeads { get; }
		double? MaximumHeads { get; }
        double? MinimumHeads { get; }
        double AbsoluteDifference { get; }
		double RelativeDifference();
		double RelativeDifferenceMinStaffBoosted();
		double RelativeDifferenceMaxStaffBoosted();
		double RelativeDifferenceBoosted();
	}

	public class SkillIntervalData : ISkillIntervalData
	{
	    private double _forecastedDemand;

	    public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, double? minimumHeads, double? maximumHeads)
		{
			Period = period;
            _forecastedDemand = forecastedDemand;
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

	    public double ForecastedDemand 
        { get
	        {
                if (Math.Abs(_forecastedDemand - 0) < 0.01)
                    _forecastedDemand = 0.01;
	            return _forecastedDemand;
	        }    
        }

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

        public double AbsoluteDifference { get { return CurrentDemand - ForecastedDemand; } }

		public double RelativeDifference()
		{
			return CurrentDemand / ForecastedDemand * -1;
		}

		public double RelativeDifferenceMinStaffBoosted()
		{
			if (!MinimumHeads.HasValue)
				return RelativeDifference();

			if (MinimumHeads.Value > 0 && CurrentHeads < MinimumHeads.Value)
				return ((CurrentHeads - MinimumHeads.Value) * 10000) + RelativeDifference();

			return RelativeDifference();

		}

		public double RelativeDifferenceMaxStaffBoosted()
		{
			if (!MaximumHeads.HasValue)
				return RelativeDifference();

			if (MaximumHeads.Value > 0 && CurrentHeads > MaximumHeads.Value)
				return ((CurrentHeads - MaximumHeads.Value) * 10000) + RelativeDifference();

			return RelativeDifference();
		}

		public double RelativeDifferenceBoosted()
		{
			double ret = 0;
			if (MinimumHeads.HasValue && MinimumHeads.Value > 0 && CurrentHeads < MinimumHeads.Value)
				ret = (CurrentHeads - MinimumHeads.Value) * 10000;
			if (MaximumHeads.HasValue && MaximumHeads.Value > 0 && CurrentHeads > MaximumHeads.Value)
				ret += (CurrentHeads - MaximumHeads.Value) * 10000;

			return ret + RelativeDifference();
		}
	}
}