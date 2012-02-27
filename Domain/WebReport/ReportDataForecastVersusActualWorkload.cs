namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportDataForecastVersusActualWorkload
	{
		public ReportDataForecastVersusActualWorkload(string period, decimal? forecastedCalls, decimal? offeredCalls, int? periodNumber)
		{
			Period = period;
			PeriodNumber = periodNumber.GetValueOrDefault();
			ForecastedCalls = forecastedCalls.GetValueOrDefault();
			OfferedCalls = offeredCalls.GetValueOrDefault();
		}

		public string Period { get; private set; }

		public int PeriodNumber { get; set; }

		public decimal ForecastedCalls { get; private set; }

		public decimal OfferedCalls { get; private set; }
	}
}