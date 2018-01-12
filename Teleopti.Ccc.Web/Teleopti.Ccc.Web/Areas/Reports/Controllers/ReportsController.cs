using System.Collections.Generic;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.Reports.Core;
using Teleopti.Ccc.Web.Areas.Reports.Models;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class ReportsController : ApiController
	{
		private readonly IReportNavigationProvider _reportNavigationProvider;

		public ReportsController(IReportNavigationProvider reportNavigationProvider)
		{
			_reportNavigationProvider = reportNavigationProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Reports/Navigations")]
		public virtual IList<ReportItemViewModel> GetReportNavigations()
		{
			return _reportNavigationProvider.GetNavigationItemViewModels();
		}

		[UnitOfWork, HttpGet, Route("api/Reports/NavigationsCategorized")]
		public virtual IList<CategorizedReportItemViewModel> GetCategorizedReportNavigations()
		{
			return _reportNavigationProvider.GetCategorizedNavigationsItemViewModels();
		}
	}
}