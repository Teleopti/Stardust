using Teleopti.Ccc.Web.Areas.MobileReports.Models.Layout;
using Teleopti.Ccc.Web.Areas.MobileReports.Models.Report;

namespace Teleopti.Ccc.Web.Areas.MobileReports.Core
{
	public interface IReportViewModelFactory
	{
		ReportViewModel CreateReportViewModel();
		ReportResponseModel GenerateReportDataResponse(ReportGenerationResult reportGenerationRequest);
	}
}