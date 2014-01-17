using System;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[MyReportPermission]
    public class MyReportController : Controller
    {
		[EnsureInPortal]
		public ViewResult Index()
        {
			return View("MyReportPartial");
        }

		//[HttpGet]
		//public JsonResult OnDates(DateTime startDate, DateTime endDate)
		//{
		//	var model = new MyReportModel
		//		{
		//			Adherence = .86,
		//			AnsweredCalls = 55,
		//			AverageAfterWork = 15,
		//			AverageHandlingTime = 135,
		//			AverageTalkTime = 120,
		//			Readiness = .88
		//		};

		//	return Json(model, JsonRequestBehavior.AllowGet);
		//}

		[HttpGet]
		public JsonResult OnDates()
		{
			var model = new MyReportModel
			{
				Adherence = .86,
				AnsweredCalls = 55,
				AverageAfterWork = 15,
				AverageHandlingTime = 135,
				AverageTalkTime = 120,
				Readiness = .88
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}
    }

	//temp here only
	public class MyReportModel
	{
		public double Adherence { get; set; }
		public double Readiness { get; set; }
		public int AverageHandlingTime { get; set; }
		public int AverageTalkTime { get; set; }
		public int AverageAfterWork { get; set; }
		public int AnsweredCalls { get; set; }
	}

}
