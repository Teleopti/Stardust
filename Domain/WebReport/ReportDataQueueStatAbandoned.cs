namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportDataQueueStatAbandoned
	{
		public ReportDataQueueStatAbandoned(string period, decimal? callsAnswered, decimal? callsAbandoned)
		{
			Period = period;
			CallsAnswered = callsAnswered.GetValueOrDefault();
			CallsAbandoned = callsAbandoned.GetValueOrDefault();
		}

		public string Period { get; private set; }

		public decimal CallsAnswered { get; private set; }

		public decimal CallsAbandoned { get; private set; }
	}
}