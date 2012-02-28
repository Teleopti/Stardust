using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Report
{
	public class ReportGenerationResult
	{
		public IEnumerable<ReportDataPeriodEntry> ReportData { get; set; }

		public IDefinedReport Report { get; set; }

		public ReportDataParam ReportInput { get; set; }
	}
}