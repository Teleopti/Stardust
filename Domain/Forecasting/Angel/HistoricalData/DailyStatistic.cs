using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData
{
	public struct DailyStatistic
	{
		private readonly int _calculatedTasks;
		private readonly DateOnly _date;

		public DailyStatistic(DateOnly date, int calculatedTasks)
		{
			_date = date;
			_calculatedTasks = calculatedTasks;
		}

		public int CalculatedTasks
		{
			get { return _calculatedTasks; }
		}

		public DateOnly Date
		{
			get { return _date; }
		}
	}
}