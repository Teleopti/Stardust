using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Reports.DataProvider;
using Teleopti.Ccc.Web.Core.RequestContext.Cookie;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ReportsController : Controller
	{
		private readonly IReportsProvider _reportsProvider;
		private readonly ISessionSpecificDataProvider _sessionSpecificDataProvider;
		public ReportsController(IReportsProvider reportsProvider, ISessionSpecificDataProvider sessionSpecificDataProvider)
		{
			_reportsProvider = reportsProvider;
			_sessionSpecificDataProvider = sessionSpecificDataProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetReports()
		{
			var data = _sessionSpecificDataProvider.GrabFromCookie();
			var realBu = data.BusinessUnitId;
			var matrixWebsiteUrl = ConfigurationManager.AppSettings["MatrixWebSiteUrl"];
			if (!string.IsNullOrEmpty(matrixWebsiteUrl) && !matrixWebsiteUrl.EndsWith("/"))
				matrixWebsiteUrl += "/";
			var reports =  new List<IApplicationFunction>(_reportsProvider.GetReports().OrderBy(x => x.LocalizedFunctionDescription));
			var reportsList = new List<ReportItem>();
			foreach (var applicationFunction in reports)
			{
				var url = string.Format(CultureInfo.CurrentCulture, "{0}Selection.aspx?ReportId={1}&BuId={2}",
					matrixWebsiteUrl, applicationFunction.ForeignId, realBu);

				reportsList.Add(new ReportItem
				{
					Url = url,
					Name = applicationFunction.LocalizedFunctionDescription,
				});
			}
			return Json(reportsList, JsonRequestBehavior.AllowGet);
		}

	}

	public class ReportItem
	{
		public string Url { get; set; }
		public string Name { get; set; }
	}
}