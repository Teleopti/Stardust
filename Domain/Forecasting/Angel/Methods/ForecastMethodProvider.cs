using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IDayWeekMonthIndexVolumes _dayWeekMonthIndexVolumes;
		private readonly ILinearRegressionTrend _linearRegressionTrend;

		public ForecastMethodProvider(IDayWeekMonthIndexVolumes dayWeekMonthIndexVolumes, ILinearRegressionTrend linearRegressionTrend)
		{
			_dayWeekMonthIndexVolumes = dayWeekMonthIndexVolumes;
			_linearRegressionTrend = linearRegressionTrend;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[]
			{
				new TeleoptiClassic(_dayWeekMonthIndexVolumes),
				new TeleoptiClassicWithTrend(_dayWeekMonthIndexVolumes, _linearRegressionTrend)
			};
		}

		public IForecastMethod Get(ForecastMethodType forecastMethodType)
		{
			switch (forecastMethodType)
			{
				case ForecastMethodType.TeleoptiClassic:
					return new TeleoptiClassic(_dayWeekMonthIndexVolumes);
				case ForecastMethodType.TeleoptiClassicWithTrend:
					return new TeleoptiClassicWithTrend(_dayWeekMonthIndexVolumes, _linearRegressionTrend);
				default:
					return null;
			}
		}
	}
}