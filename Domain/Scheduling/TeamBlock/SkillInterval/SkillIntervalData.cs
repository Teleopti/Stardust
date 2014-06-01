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
		double RelativeDifferenceBoosted();
		double MinMaxBoostFactorForStandardDeviation { get; set; }
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
			MinMaxBoostFactorForStandardDeviation = calculateMinMaxBoostFactorForStandardDeviationFactor();
		}

		private double calculateMinMaxBoostFactor()
		{
			double minHeadValue = 0;
			double maxHeadValue = 0;
			if (MinimumHeads.HasValue)
			{
				if (CurrentHeads < MinimumHeads.Value)
					minHeadValue = (MinimumHeads.Value - CurrentHeads);
			}
			if (MaximumHeads.HasValue)
			{
				if (CurrentHeads >= MaximumHeads.Value)
					maxHeadValue = MaximumHeads.Value - (CurrentHeads + 1);
			}
			return minHeadValue + maxHeadValue;
		}

		private double calculateMinMaxBoostFactorForStandardDeviationFactor()
		{
			double minHeadValue = 0;
			double maxHeadValue = 0;
			if (MinimumHeads.HasValue)
			{
				if (CurrentHeads < MinimumHeads.Value)
					minHeadValue = (MinimumHeads.Value - CurrentHeads);
			}
			if (MaximumHeads.HasValue)
			{
				if (CurrentHeads >= MaximumHeads.Value)
					maxHeadValue = MaximumHeads.Value - CurrentHeads;
			}
			return minHeadValue + maxHeadValue;
		}

		public double ForecastedDemand
		{
			get
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

		public double MinMaxBoostFactorForStandardDeviation { get; set; }

		public TimeSpan Resolution()
		{
			return Period.EndDateTime.Subtract(Period.StartDateTime);
		}

		public double AbsoluteDifference { get { return -CurrentDemand; } }

		public double RelativeDifference()
		{
			return CurrentDemand / ForecastedDemand * -1;
		}

		public double RelativeDifferenceBoosted()
		{
			double ret = MinMaxBoostFactorForStandardDeviation * 10000;			
			return ret + RelativeDifference();
		}
	}
}