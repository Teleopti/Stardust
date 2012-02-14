namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	public class ReportResponse
	{
		public ReportInfo ReportInfo { get; set; }

		public ReportTableRowViewModel[] ReportData { get; set; }

		public ReportChartInfo ReportChart { get; set; }
	}
}