using System.Threading.Tasks;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Insights.Core.DataProvider;
using Teleopti.Ccc.Web.Areas.Insights.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Insights.Controllers
{
	public class InsightsController : ApiController
	{
		private readonly IReportProvider _reportProvider;
		private readonly Core.DataProvider.IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public InsightsController(IReportProvider reportProvider,
			Core.DataProvider.IPermissionProvider permissionProvider,
			ILoggedOnUser loggedOnUser)
		{
			_reportProvider = reportProvider;
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		[HttpGet, UnitOfWork, Route("api/Insights/Permission")]
		public virtual InsightsPermission GetPermission()
		{
			return _permissionProvider.GetInsightsPermission(_loggedOnUser.CurrentUser(), DateOnly.Today);
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
