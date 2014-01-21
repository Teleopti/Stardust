using System;
using System.Collections.Generic;
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

		[HttpGet]
		public JsonResult OnDates(MyReportInput input)
		{
			var model = new MyReportModel
			{
				Adherence = .86,
				AnsweredCalls = 30 + input.OnDate.Day,
				AverageAfterWork = 15,
				AverageHandlingTime = 135,
				AverageTalkTime = 120,
				Readiness = .88,
				DisplayDate = input.OnDate.ToShortDateString()
			};

			return Json(model, JsonRequestBehavior.AllowGet);
		}

		//[HttpGet]
		//public JsonResult WeekValues(DateTime startDate)
		//{
		//	var models = new List<MyReportModel>
		//		{
		//			new MyReportModel
		//				{
		//					Adherence = .86,
		//					AnsweredCalls = 55,
		//					AverageAfterWork = 15,
		//					AverageHandlingTime = 135,
		//					AverageTalkTime = 120,
		//					Readiness = .88,
		//					DisplayDate = startDate.ToShortDateString()
		//				},
		//			new MyReportModel
		//				{
		//					Adherence = .88,
		//					AnsweredCalls = 50,
		//					AverageAfterWork = 12,
		//					AverageHandlingTime = 140,
		//					AverageTalkTime = 128,
		//					Readiness = .87,
		//					DisplayDate = startDate.AddDays(1).ToShortDateString()
		//				},
		//			new MyReportModel
		//				{
		//					Adherence = .85,
		//					AnsweredCalls = 66,
		//					AverageAfterWork = 10,
		//					AverageHandlingTime = 130,
		//					AverageTalkTime = 120,
		//					Readiness = .92,
		//					DisplayDate = startDate.AddDays(2).ToShortDateString()
		//				}
		//		};

		//	return Json(models, JsonRequestBehavior.AllowGet);
		//}
    }

	
    }

	//temp here only
	public class MyReportModel
	{
		public string DisplayDate { get; set; }
		public double Adherence { get; set; }
		public double Readiness { get; set; }
		public int AverageHandlingTime { get; set; }
		public int AverageTalkTime { get; set; }
		public int AverageAfterWork { get; set; }
		public int AnsweredCalls { get; set; }
	}

public class MyReportInput
{
	public DateTime OnDate { get; set; }
	public bool ShowWeek { get; set; }
}


