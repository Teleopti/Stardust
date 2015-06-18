using System;
using System.Linq;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class TeleoptiClassicWithTrend : TeleoptiClassicBase
	{
		private readonly ILinearRegressionTrend _linearRegressionTrend;

		public TeleoptiClassicWithTrend(IDayWeekMonthIndexVolumes indexVolumes, ILinearRegressionTrend linearRegressionTrend)
			: base(indexVolumes)
		{
			_linearRegressionTrend = linearRegressionTrend;
		}

		public override ForecastResult Forecast(ITaskOwnerPeriod historicalData, DateOnlyPeriod futurePeriod)
		{
			var trend = _linearRegressionTrend.CalculateTrend(historicalData);
			var forecastResult = base.Forecast(historicalData, futurePeriod);
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