using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.MyReport.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[MyReportPermission]
	public class MyReportController : Controller
	{
		private readonly IMyReportViewModelFactory _myReportViewModelFactory;

		public MyReportController(IMyReportViewModelFactory myReportViewModelFactory)
		{
			_myReportViewModelFactory = myReportViewModelFactory;
		}

		[EnsureInPortal]
		public ViewResult Index()
		{
			return View("MyReportPartial");
		}

		[HttpGet]
		[UnitOfWorkAction]
		public JsonResult OnDates(DateTime date)
		{
			return Json(_myReportViewModelFactory.CreateDailyMetricsViewModel(new DateOnly(date)), JsonRequestBehavior.AllowGet);
		}
	}
}