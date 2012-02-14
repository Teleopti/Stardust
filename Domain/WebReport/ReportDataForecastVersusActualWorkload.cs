namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportDataForecastVersusActualWorkload
	{
		public ReportDataForecastVersusActualWorkload(string period, decimal? forecastedCalls, decimal? offeredCalls)
		{
			Period = period;
			ForecastedCalls = forecastedCalls.GetValueOrDefault();
			OfferedCalls = offeredCalls.GetValueOrDefault();
		}

		public string Period { get; private set; }

		public decimal ForecastedCalls { get; private set; }

		public decimal OfferedCalls { get; private set; }
	}
}