using System;
using System.Threading.Tasks;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider
{
	public interface IReportProvider
	{
		Task<ReportModel[]> GetReports();
		Task<EmbedReportConfig> GetReportConfig(Guid reportId);
		Task<EmbedReportConfig> CreateReport(string newReportName);
		Task<EmbedReportConfig> CloneReport(Guid reportId, string newReportName);
		Task<bool> DeleteReport(Guid reportId);
	}
}