namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportDataQueueStatAbandoned
	{
		public ReportDataQueueStatAbandoned(string period, decimal? callsAnswered, decimal? callsAbandoned, int? periodNumber)
		{
			Period = period;
			PeriodNumber = periodNumber.GetValueOrDefault();
			CallsAnswered = callsAnswered.GetValueOrDefault();
			CallsAbandoned = callsAbandoned.GetValueOrDefault();
		}

		public string Period { get; private set; }

		public int PeriodNumber { get; set; }

		public decimal CallsAnswered { get; private set; }

		public decimal CallsAbandoned { get; private set; }
	}
}