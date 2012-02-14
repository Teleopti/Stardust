using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public interface IReportRequestValidator
	{
		ReportDataFetchResult FetchData(ReportRequestModel request);
	}
}