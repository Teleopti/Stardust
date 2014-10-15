using Teleopti.Ccc.Domain.Forecasting.Angel.HistoricalData;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class QuickForecaster : IQuickForecaster
	{
		private readonly IHistoricalDataProvider _historicalDataProvider;

		public QuickForecaster(IHistoricalDataProvider historicalDataProvider)
		{
			_historicalDataProvider = historicalDataProvider;
		}

		public void Execute(IWorkload workload, DateOnlyPeriod period)
		{
			_historicalDataProvider.Calculate(workload, period);
		}
	}
}