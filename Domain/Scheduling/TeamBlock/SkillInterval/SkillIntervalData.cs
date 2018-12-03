using System;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Secrets.WorkShiftPeriodValueCalculator;

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
		public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, double? minimumHeads, double? maximumHeads)
		{
			Period = period;

			ForecastedDemand = Math.Abs(forecastedDemand) < 0.01 ? 0.01 : forecastedDemand;

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

		public double ForecastedDemand { get; }

		public DateTimePeriod Period { get; }

		public double CurrentHeads { get; }

		public double? MaximumHeads { get; }

		public double? MinimumHeads { get; }

		public double CurrentDemand { get; }

		public double MinMaxBoostFactor { get; set; }

		public double MinMaxBoostFactorForStandardDeviation { get; set; }

		public TimeSpan Resolution()
		{
			return Period.ElapsedTime();
		}

		public double AbsoluteDifference => -CurrentDemand;

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