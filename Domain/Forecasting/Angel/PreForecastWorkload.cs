using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Ccc.Domain.Forecasting.Angel.Historical;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class PreForecastWorkload : IPreForecastWorkload
	{
		private readonly IHistoricalData _historicalData;
		private readonly IForecastMethodProvider _forecastMethodProvider;
		private readonly IHistoricalPeriodProvider _historicalPeriodProvider;

		public PreForecastWorkload(IHistoricalData historicalData, IForecastMethodProvider forecastMethodProvider, IHistoricalPeriodProvider historicalPeriodProvider)
		{
			_historicalData = historicalData;
			_forecastMethodProvider = forecastMethodProvider;
			_historicalPeriodProvider = historicalPeriodProvider;
		}

		public IDictionary<DateOnly, IDictionary<ForecastMethodType, double>> PreForecast(IWorkload workload, DateOnlyPeriod futurePeriod)
		{
			var dic = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			var historicalData = _historicalData.Fetch(workload, _historicalPeriodProvider.PeriodForForecast());
			if (!historicalData.TaskOwnerDayCollection.Any())
				return dic;
			var forecastMethods = _forecastMethodProvider.All();
			var isFirstMethod = true;
			foreach (var forecastMethod in forecastMethods)
			{
				var forecastResult = forecastMethod.Forecast(historicalData, futurePeriod);
				if (isFirstMethod)
				{
					foreach (var forecastingTarget in forecastResult)
					{
						var dictionary = new Dictionary<ForecastMethodType, double> {{forecastMethod.Id, forecastingTarget.Tasks}};
						dic.Add(forecastingTarget.CurrentDate, dictionary);
					}
					isFirstMethod = false;
				}
				else
				{
					foreach (var forecastingTarget in forecastResult)
					{
						dic[forecastingTarget.CurrentDate].Add(forecastMethod.Id, forecastingTarget.Tasks);
					}
				}
			}
			return dic;
		}
	}
}