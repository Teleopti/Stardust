using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly IDayWeekMonthIndexVolumes _dayWeekMonthIndexVolumes;
		private readonly ILinearTrendCalculator _linearTrendCalculator;

		public ForecastMethodProvider(IDayWeekMonthIndexVolumes dayWeekMonthIndexVolumes, ILinearTrendCalculator linearTrendCalculator)
		{
			_dayWeekMonthIndexVolumes = dayWeekMonthIndexVolumes;
			_linearTrendCalculator = linearTrendCalculator;
		}

		public IForecastMethod[] All()
		{
			return new IForecastMethod[]
			{
				new TeleoptiClassic(_dayWeekMonthIndexVolumes),
				new TeleoptiClassicWithTrend(_dayWeekMonthIndexVolumes, _linearTrendCalculator)
			};
		}

		public IForecastMethod Get(ForecastMethodType forecastMethodType)
		{
			switch (forecastMethodType)
			{
				case ForecastMethodType.TeleoptiClassic:
					return new TeleoptiClassic(_dayWeekMonthIndexVolumes);
				case ForecastMethodType.TeleoptiClassicWithTrend:
					return new TeleoptiClassicWithTrend(_dayWeekMonthIndexVolumes, _linearTrendCalculator);
				default:
					return null;
			}
		}
	}
}