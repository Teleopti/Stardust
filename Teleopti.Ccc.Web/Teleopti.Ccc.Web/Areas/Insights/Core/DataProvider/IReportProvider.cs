using System.Threading.Tasks;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IReportProvider
	{
		Task<ReportModel[]> GetReports();
		Task<EmbedReportConfig> GetReportConfig(string reportId);
		Task<EmbedReportConfig> CloneReport(string reportId);
	}
}