namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportDataServiceLevelAgentsReady
	{
		public ReportDataServiceLevelAgentsReady(string period, decimal? scheduledAgentsReady, decimal? agentsReady,
												  decimal? serviceLevel, int? periodNumber)
		{
			Period = period;
			PeriodNumber = periodNumber.GetValueOrDefault();
			ScheduledAgentsReady = scheduledAgentsReady.GetValueOrDefault();
			AgentsReady = agentsReady.GetValueOrDefault();
			ServiceLevel = serviceLevel.GetValueOrDefault();
		}

		public string Period { get; private set; }

		public int PeriodNumber { get; set; }

		public decimal ScheduledAgentsReady { get; private set; }

		public decimal AgentsReady { get; private set; }

		public decimal ServiceLevel { get; private set; }
	}
}