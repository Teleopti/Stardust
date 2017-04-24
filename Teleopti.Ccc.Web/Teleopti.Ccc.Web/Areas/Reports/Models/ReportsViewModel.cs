namespace Teleopti.Ccc.Web.Areas.Reports.Models
{
	public class ReportItem
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public bool IsWebReport { get; set; }
	}

	public class CategorizedReportItem : ReportItem
	{
		public string Category { get; set; }
	}

}