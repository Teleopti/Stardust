namespace Teleopti.Ccc.Domain.WebReport
{
	public class ReportMobileReportInit
	{
		public int Scenario { get; set; }

		public int TimeZone { get; set; }

		public int IntervalFrom { get; set; }

		public int IntervalTo { get; set; }

		public string SkillSet { get; set; }

		public string WorkloadSet { get; set; }

		public int ServiceLevelCalculationId { get; set; }
	}
}