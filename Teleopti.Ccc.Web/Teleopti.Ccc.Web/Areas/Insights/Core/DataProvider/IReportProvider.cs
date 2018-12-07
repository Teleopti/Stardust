using System.Threading.Tasks;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IReportProvider
	{
		Task<ReportModel[]> GetReports();
		Task<EmbedReportConfig> GetReportConfig(string reportId);
		Task<EmbedReportConfig> CreateReport(string newReportName);
		Task<EmbedReportConfig> CloneReport(string reportId, string newReportName);
		Task<bool> DeleteReport(string reportId);
	}
}