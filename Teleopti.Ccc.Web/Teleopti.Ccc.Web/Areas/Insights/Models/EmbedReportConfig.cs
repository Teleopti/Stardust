using System;

namespace Teleopti.Ccc.Web.Areas.Insights.Models
{
	public class EmbedReportConfig
	{
		public string ReportId { get; set; }
		public string ReportName { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? CreatedOn { get; set; }
		public string UpdatedBy { get; set; }
		public DateTime? UpdatedOn { get; set; }
		public string ReportUrl { get; set; }
		public string TokenType { get; set; }
		public string AccessToken { get; set; }
		public DateTime? Expiration { get; set; }
	}
}