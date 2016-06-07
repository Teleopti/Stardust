using System;

namespace Teleopti.Ccc.Web.Areas.Reports.Models
{
	public class ReportsViewModel
	{

	}

	public class NavigationItem
	{
		public string Title { get; set; }
		public string Controller { get; set; }
		public string Action { get; set; }

		public bool PayAttention { get; set; }
		public string TitleCount { get; set; }
	}

	public class ReportNavigationItem : NavigationItem
	{
		public string Url { get; set; }
		public bool IsWebReport { get; set; }
		public bool IsDivider { get; set; }
		public Guid Id { get; set; }
	}
}