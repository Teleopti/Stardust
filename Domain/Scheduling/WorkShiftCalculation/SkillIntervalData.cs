namespace Teleopti.Ccc.Domain.Scheduling.WorkShiftCalculation
{
	public interface ISkillIntervalData
	{
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
		private readonly double _currentDemand;

		public SkillIntervalData(double currentDemand, double currentHeads, int? minimumHeads, int? maximumHeads)
		{
			_currentDemand = currentDemand;
			_currentHeads = currentHeads;
			_minimumHeads = minimumHeads;
			_maximumHeads = maximumHeads;
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