using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Domain.Forecasting.Angel.Accuracy
{
	public interface IHistoricalPeriodProvider
	{
		DateOnlyPeriod PeriodForEvaluate();
		DateOnlyPeriod PeriodForForecast();
	}

	public class HistoricalPeriodProvider : IHistoricalPeriodProvider
	{
		private readonly INow _now;

		public HistoricalPeriodProvider(INow now)
		{
			_now = now;
		}

		public DateOnlyPeriod PeriodForEvaluate()
		{
			var nowDate = _now.LocalDateOnly();
			return new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-2)), nowDate);
		}

		public DateOnlyPeriod PeriodForForecast()
		{
			var nowDate = _now.LocalDateOnly();
			return new DateOnlyPeriod(new DateOnly(nowDate.Date.AddYears(-1)), nowDate);
		}
	}
}