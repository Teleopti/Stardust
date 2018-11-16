using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Insights.Models;

namespace Teleopti.Ccc.Web.Areas.Insights.Controllers
{
	public class InsightsController : ApiController
	{
		private readonly IReportProvider _reportProvider;

		public InsightsController(IReportProvider reportProvider)
		{
			_reportProvider = reportProvider;
		}

		[HttpGet, Route("api/Insights/ReportConfig")]
		public virtual async Task<EmbedReportConfig> GetReportConfig(string reportId)
		{
			return await _reportProvider.GetReportConfig(reportId);
		}

		[HttpGet, Route("api/Insights/Reports")]
		public virtual async Task<ReportModel[]> GetReports()
		{
			return await _reportProvider.GetReports();
		}

		[HttpGet, Route("api/Insights/CloneReport")]
		public virtual async Task<EmbedReportConfig> CloneReport(string reportId)
		{
			return await _reportProvider.CloneReport(reportId);
		}
	}
}
