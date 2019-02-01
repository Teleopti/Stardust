using System;

namespace Teleopti.Ccc.Web.Areas.Insights.Models
{
	public class ReportModel
	{
		public string Id { get; set; }
		public string Name { get; set; }
		// public string DatasetId { get; set; }
		// public string DatasetId { get; set; }
		// public string WebUrl { get; set; }
		public string EmbedUrl { get; set; }
		public string CreatedBy { get; set; }
		public DateTime? CreatedOn { get; set; }
		public string UpdatedBy { get; set; }
		public DateTime? UpdatedOn { get; set; }
	}
}