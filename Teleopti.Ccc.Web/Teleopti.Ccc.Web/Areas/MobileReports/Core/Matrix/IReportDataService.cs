using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core.Matrix
{
	public interface IReportDataService
	{
		IEnumerable<ReportDataPeriodEntry> GetAnsweredAndAbandoned(ReportDataParam param);
		IEnumerable<ReportDataPeriodEntry> GetForecastVersusActualWorkload(ReportDataParam param);
		IEnumerable<ReportDataPeriodEntry> GetScheduledAndActual(ReportDataParam param);
		IEnumerable<ReportDataPeriodEntry> GetServiceLevelAgent(ReportDataParam param);
	}
}