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
	}
}