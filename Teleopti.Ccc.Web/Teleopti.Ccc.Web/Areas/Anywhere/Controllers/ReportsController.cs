using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class ReportsController : Controller
	{

		private readonly IReportItemsProvider _reportItemsProvider;
		public ReportsController(IReportItemsProvider reportItemsProvider)
		{
			_reportItemsProvider = reportItemsProvider;
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult GetReports()
		{
			return Json(_reportItemsProvider.GetReportItems(), JsonRequestBehavior.AllowGet);
		}

	}
}