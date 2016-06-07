using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Portal;
using Teleopti.Ccc.Web.Areas.Reports.Core;

namespace Teleopti.Ccc.Web.Areas.Reports.Controllers
{
	public class ReportsController : ApiController
	{
		private readonly IReportNavigationProvider reportNavigationProvider;

		public ReportsController(IReportNavigationProvider reportNavigationProvider)
		{
			this.reportNavigationProvider = reportNavigationProvider;
		}

		[UnitOfWork, HttpGet, Route("api/Reports/Navigations")]
		public virtual IList<ReportNavigationItem> GetReportNavigations()
		{
			return reportNavigationProvider.GetNavigationItems();
		}

	}
}