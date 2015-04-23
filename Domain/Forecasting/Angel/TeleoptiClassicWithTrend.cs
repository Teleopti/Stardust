using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel
{
	public class TeleoptiClassicWithTrend : TeleoptiClassicBase
	{
		private readonly ILinearRegressionTrend _linearRegressionTrend;

		public TeleoptiClassicWithTrend(IIndexVolumes indexVolumes, ILinearRegressionTrend linearRegressionTrend) : base(indexVolumes)
		{
			_linearRegressionTrend = linearRegressionTrend;
		}

		public override IList<IForecastingTarget> Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var trend = _linearRegressionTrend.CalculateTrend(historicalData);
			var forecastWithoutTrend = base.Forecast(historicalData, futurePeriod);
			var averageTasks = forecastWithoutTrend.Average(x => x.Tasks);
			foreach (var forecastingTarget in forecastWithoutTrend)
			{
				forecastingTarget.Tasks = Math.Max(0, forecastingTarget.Tasks + forecastingTarget.CurrentDate.Subtract(LinearTrend.StartDate).Days * trend.Slope + trend.Intercept - averageTasks);
			}
			return forecastWithoutTrend;
		}

		public override ForecastMethodType Id {
			get
			{
				return ForecastMethodType.TeleoptiClassicWithTrend;
			}
		}
	}
}