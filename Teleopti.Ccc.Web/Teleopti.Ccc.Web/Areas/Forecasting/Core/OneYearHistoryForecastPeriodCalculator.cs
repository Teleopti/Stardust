using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Forecasting.Core
{
	public class OneYearHistoryForecastPeriodCalculator : IForecastHistoricalPeriodCalculator
	{
		private readonly INow _now;

		public OneYearHistoryForecastPeriodCalculator(INow now)
		{
			_now = now;
		}

		public DateOnlyPeriod HistoricalPeriod(DateOnlyPeriod futurePeriod)
		{
			var nowDate = _now.LocalDateOnly();
			var historicalPeriodStartTime = new DateOnly(nowDate.Date.AddYears(-1));
			return new DateOnlyPeriod(historicalPeriodStartTime, nowDate);
		}

		public DateOnlyPeriod OneYearHistoricalPeriod(DateOnlyPeriod futurePeriod)
		{
			var historicalPeriodStartTime = new DateOnly(futurePeriod.StartDate.Date.AddYears(-1));
			return new DateOnlyPeriod(historicalPeriodStartTime, futurePeriod.StartDate);
		}
	}
}