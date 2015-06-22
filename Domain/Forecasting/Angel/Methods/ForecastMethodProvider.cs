using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly ILinearTrendCalculator _linearTrendCalculator;

		public ForecastMethodProvider(ILinearTrendCalculator linearTrendCalculator)
		{
			_linearTrendCalculator = linearTrendCalculator;
		}

		public IForecastMethod[] Calculate(DateOnlyPeriod period)
		{
			return new[]
			{
				Get(ForecastMethodType.TeleoptiClassicLongTerm),
				Get(ForecastMethodType.TeleoptiClassicLongTermWithTrend)
			};
		}

		public IForecastMethod Get(ForecastMethodType forecastMethodType)
		{
			switch (forecastMethodType)
			{
				case ForecastMethodType.TeleoptiClassicShortTerm:
					return new TeleoptiClassicShortTerm();
				case ForecastMethodType.TeleoptiClassicMediumTerm:
					return new TeleoptiClassicMediumTerm();
				case ForecastMethodType.TeleoptiClassicMediumTermWithTrend:
					return new TeleoptiClassicMediumTermWithTrend(_linearTrendCalculator);
				case ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth:
					return new TeleoptiClassicMediumTermWithDayInMonth();
				case ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonthWithTrend:
					return new TeleoptiClassicMediumTermWithDayInMonthWithTrend(_linearTrendCalculator);
				case ForecastMethodType.TeleoptiClassicLongTerm:
					return new TeleoptiClassicLongTerm();
				case ForecastMethodType.TeleoptiClassicLongTermWithTrend:
					return new TeleoptiClassicLongTermWithTrend(_linearTrendCalculator);
				case ForecastMethodType.TeleoptiClassicLongTermWithDayInMonth:
					return new TeleoptiClassicLongTermWithDayInMonth();
				case ForecastMethodType.TeleoptiClassicLongTermWithDayInMonthWithTrend:
					return new TeleoptiClassicLongTermWithDayInMonthWithTrend(_linearTrendCalculator);
				default:
					return null;
			}
		}
	}

}