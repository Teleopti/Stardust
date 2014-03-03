using System;
using Teleopti.Ccc.Domain.Common;
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
		TimeSpan Resolution();
		double RelativeDifference();
		double RelativeDifferenceMinStaffBoosted();
		double RelativeDifferenceMaxStaffBoosted();
		double RelativeDifferenceBoosted();
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

		public TimeSpan Resolution()
		{
			return Period.EndDateTime.Subtract(Period.StartDateTime);
		}

		public double RelativeDifference()
		{
			if (ForecastedDemand < 0.001)
				ForecastedDemand = 0.001;

			return CurrentDemand/ForecastedDemand*-1;
		}

		public double RelativeDifferenceMinStaffBoosted()
		{
			if(!MinimumHeads.HasValue)
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