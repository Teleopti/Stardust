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

		public override IList<IForecastingTarget> Forecast(TaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod, bool removeOutliers)
		{
			var trend = _linearRegressionTrend.CalculateTrend(historicalData);
			var forecastWithoutTrend = base.Forecast(historicalData, futurePeriod, removeOutliers);

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