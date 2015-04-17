using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class PreForecastWorkload : IPreForecastWorkload
	{
		private readonly IForecastMethodProvider _forecastMethodProvider;

		public PreForecastWorkload(IForecastMethodProvider forecastMethodProvider)
		{
			_forecastMethodProvider = forecastMethodProvider;
		}

		public IDictionary<DateOnly, IDictionary<ForecastMethodType, double>> PreForecast(IWorkload workload, DateOnlyPeriod futurePeriod, TaskOwnerPeriod historicalDataForForecasting)
		{
			var dic = new Dictionary<DateOnly, IDictionary<ForecastMethodType, double>>();
			if (!historicalDataForForecasting.TaskOwnerDayCollection.Any())
				return dic;
			var forecastMethods = _forecastMethodProvider.All();
			var isFirstMethod = true;
			foreach (var forecastMethod in forecastMethods)
			{
				var forecastResult = forecastMethod.Forecast(historicalDataForForecasting, futurePeriod);
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