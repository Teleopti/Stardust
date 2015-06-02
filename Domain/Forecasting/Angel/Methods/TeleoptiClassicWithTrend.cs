using System;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Outlier;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassicWithTrend : TeleoptiClassicBase
	{
		private readonly ILinearRegressionTrend _linearRegressionTrend;

		public TeleoptiClassicWithTrend(IIndexVolumes indexVolumes, ILinearRegressionTrend linearRegressionTrend, IOutlierRemover outlierRemover)
			: base(indexVolumes, outlierRemover)
		{
			_linearRegressionTrend = linearRegressionTrend;
		}

		public override ForecastResult Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod, bool removeOutliers)
		{
			var trend = _linearRegressionTrend.CalculateTrend(historicalData);
			var forecastResult = base.Forecast(historicalData, futurePeriod, removeOutliers);
			var forecastTargets = forecastResult.ForecastingTargets;

			var averageTasks = forecastTargets.Average(x => x.Tasks);
			foreach (var forecastingTarget in forecastTargets)
			{
				forecastingTarget.Tasks = Math.Max(0, forecastingTarget.Tasks + forecastingTarget.CurrentDate.Subtract(LinearTrend.StartDate).Days * trend.Slope + trend.Intercept - averageTasks);
			}
			return forecastResult;
		}

		public override ForecastMethodType Id {
			get
			{
				return ForecastMethodType.TeleoptiClassicWithTrend;
			}
		}
	}
}