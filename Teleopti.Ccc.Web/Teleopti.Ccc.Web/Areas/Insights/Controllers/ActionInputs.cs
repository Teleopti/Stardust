namespace Teleopti.Ccc.Web.Areas.Insights.Controllers
{

	public class ReportConfigInput
	{
		public string ReportId { get; set; }
		public int ViewMode { get; set; }
	}

	public class UpdateReportInput
	{
		public string ReportId { get; set; }
		public string ReportName { get; set; }
	}

	public class CloneReportInput
	{
		public string ReportId { get; set; }
		public string NewReportName { get; set; }
	}
}