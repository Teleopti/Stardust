using System;
using Teleopti.Ccc.Domain.Forecasting.Angel.Trend;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Methods
{
	public class ForecastMethodProvider : IForecastMethodProvider
	{
		private readonly ILinearTrendCalculator _linearTrendCalculator;
		private const int dayCountPerMonth = 30;
		private const int dayCountPerYear = 365;

		public ForecastMethodProvider(ILinearTrendCalculator linearTrendCalculator)
		{
			_linearTrendCalculator = linearTrendCalculator;
		}

		public IForecastMethod[] Calculate(DateOnlyPeriod period)
		{
			var length = period.EndDate.Date - period.StartDate.Date;
			if (length < TimeSpan.FromDays(2*dayCountPerMonth))
			{
				return new[]
				{
					Get(ForecastMethodType.TeleoptiClassicShortTerm)
				};
			}
			if (length < TimeSpan.FromDays(dayCountPerYear+dayCountPerMonth))
			{
				return new[]
				{
					Get(ForecastMethodType.TeleoptiClassicMediumTerm),
					Get(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth)
				};
			}
			if (length < TimeSpan.FromDays(2*dayCountPerYear))
			{
				return new[]
				{
					Get(ForecastMethodType.TeleoptiClassicMediumTerm),
					Get(ForecastMethodType.TeleoptiClassicMediumTermWithTrend),
					Get(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonth),
					Get(ForecastMethodType.TeleoptiClassicMediumTermWithDayInMonthWithTrend)
				};
			}
			return new[]
			{
				Get(ForecastMethodType.TeleoptiClassicLongTerm),
				Get(ForecastMethodType.TeleoptiClassicLongTermWithTrend),
				Get(ForecastMethodType.TeleoptiClassicLongTermWithDayInMonth),
				Get(ForecastMethodType.TeleoptiClassicLongTermWithDayInMonthWithTrend)
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