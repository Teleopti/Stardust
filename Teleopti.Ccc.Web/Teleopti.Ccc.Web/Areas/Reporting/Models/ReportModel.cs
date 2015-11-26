using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;

namespace Teleopti.Ccc.Web.Areas.Reporting.Models
{
	public class ReportModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<ReportNavigationItem> ReportNavigationItems { get; set; }
		public string HelpUrl { get; set; }
		public string CurrentLogonAgentName { get; set; }
		public string CurrentBuName { get; set; }
		public bool UseOpenXml { get; set; }
	}
}