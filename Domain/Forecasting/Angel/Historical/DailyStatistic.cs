using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Historical
{
	public struct DailyStatistic
	{
		private readonly int _calculatedTasks;
		private readonly DateOnly _date;
		private readonly double _averageTaskTimeSeconds;
		private readonly double _averageAfterTaskTimeSeconds;

		public DailyStatistic(DateOnly date, int calculatedTasks, double averageTaskTimeSeconds, double averageAfterTaskTimeSeconds)
		{
			_date = date;
			_calculatedTasks = calculatedTasks;
			_averageTaskTimeSeconds = averageTaskTimeSeconds;
			_averageAfterTaskTimeSeconds = averageAfterTaskTimeSeconds;
		}

		public int CalculatedTasks
		{
			get { return _calculatedTasks; }
		}

		public DateOnly Date
		{
			get { return _date; }
		}

		public double AverageTaskTimeSeconds
		{
			get { return _averageTaskTimeSeconds; }
		}

		public double AverageAfterTaskTimeSeconds
		{
			get { return _averageAfterTaskTimeSeconds; }
		}
	}
}