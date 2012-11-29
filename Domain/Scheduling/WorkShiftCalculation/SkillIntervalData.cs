using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ISkillIntervalData
	{
		double ForecastedDemand { get; }
		DateTimePeriod Period { get; }
		double CurrentHeads { get; }
		int? MaximumHeads { get; }
		int? MinimumHeads { get; }
		double CurrentDemand { get; }
	}

	public class SkillIntervalData : ISkillIntervalData
	{
		private readonly double _currentHeads;
		private readonly int? _minimumHeads;
		private readonly int? _maximumHeads;
		private readonly DateTimePeriod _period;
		private readonly double _forecastedDemand;
		private readonly double _currentDemand;

		public SkillIntervalData(DateTimePeriod period, double forecastedDemand, double currentDemand, double currentHeads, int? minimumHeads, int? maximumHeads)
		{
			_period = period;
			_forecastedDemand = forecastedDemand;
			_currentDemand = currentDemand;
			_currentHeads = currentHeads;
			_minimumHeads = minimumHeads;
			_maximumHeads = maximumHeads;
		}

		public double ForecastedDemand
		{
			get { return _forecastedDemand; }
		}

		public DateTimePeriod Period
		{
			get { return _period; }
		}

		public double CurrentHeads
		{
			get { return _currentHeads; }
		}

		public int? MaximumHeads
		{
			get { return _maximumHeads; }
		}

		public int? MinimumHeads
		{
			get { return _minimumHeads; }
		}

		public double CurrentDemand
		{
			get { return _currentDemand; }
		}

	}
}