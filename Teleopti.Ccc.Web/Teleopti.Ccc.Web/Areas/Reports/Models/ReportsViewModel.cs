namespace Teleopti.Ccc.Web.Areas.Reports.Models
{
	public class ReportItemViewModel
	{
		public string Url { get; set; }
		public string Name { get; set; }
		public bool IsWebReport { get; set; }
		public string ForeignId { get; set; }
	}

	public class CategorizedReportItemViewModel : ReportItemViewModel
	{
		public string Category { get; set; }
	}
}