using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.TeamBlock
{
	public interface ISkillIntervalData
	{
		double ForecastedDemand { get; set; }
		DateTimePeriod Period { get; }
		double CurrentHeads { get; set; }
		int? MaximumHeads { get; set; }
		int? MinimumHeads { get; set; }
		double CurrentDemand { get; set; }
	}

	public class SkillIntervalData : ISkillIntervalData
	{
		private readonly DateTimePeriod _period;

		public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, int? minimumHeads, int? maximumHeads)
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

		public int? MaximumHeads { get; set; }

		public int? MinimumHeads { get; set; }

		public double CurrentDemand { get; set; }
	}
}