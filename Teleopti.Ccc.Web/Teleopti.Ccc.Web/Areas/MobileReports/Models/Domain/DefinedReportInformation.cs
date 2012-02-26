using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;


namespace Teleopti.Ccc.Web.Areas.MobileReports.Models.Domain
{
	using System;

	public delegate IEnumerable<ReportDataPeriodEntry> GenerateReportData(
		IReportDataService dataService, ReportDataParam dataServiceParams);

	public class ReportMetaInfo
	{
		public string[] SeriesResourceKeys { get; set; }
		public int[] SeriesFixedDecimalHint { get; set; }
		public string[] ChartTypeHint { get; set; }

	}

	public interface IDefinedReport
	{
		GenerateReportData GenerateReport { get; set; }
		Guid ForeignId { get; set; }
		string ReportId { get; set; }
		ReportMetaInfo ReportInfo { get; set; }
		string ReportNameResourceKey { get; set; }

		string FunctionCode { get; set; }
	}

	public sealed class DefinedReportInformation : IDefinedReport
	{
		#region IDefinedReport Members

		public Guid ForeignId { get; set; }
		public string ReportId { get; set; }
		public string ReportNameResourceKey { get; set; }

		public ReportMetaInfo ReportInfo { get; set; }

		public string FunctionCode { get; set; }

		public GenerateReportData GenerateReport { get; set; }

		#endregion
	}
}