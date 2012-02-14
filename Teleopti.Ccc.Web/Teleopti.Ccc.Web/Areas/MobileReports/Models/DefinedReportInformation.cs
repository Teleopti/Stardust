using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Models
{
	public delegate IEnumerable<ReportDataPeriodEntry> GenerateReportData(
		IReportDataService dataService, ReportDataParam dataServiceParams);

	public interface IDefinedReport
	{
		GenerateReportData GenerateReport { get; set; }
		string ReportName { get; set; }
		string ReportId { get; set; }
		string ReportNameResourceKey { get; set; }
		string []LegendResourceKeys { get; set; }
		string FunctionCode { get; set; }
	}

	public class DefinedReportInformation : IDefinedReport
	{
		#region IDefinedReport Members

		public string ReportName { get; set; }
		public string ReportId { get; set; }
		public string ReportNameResourceKey { get; set; }

		public string[] LegendResourceKeys { get; set; }
		

		public string FunctionCode { get; set; }

		public GenerateReportData GenerateReport { get; set; }

		#endregion
	}
}