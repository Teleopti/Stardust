using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.UserTexts;
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
				AdherenceText = Resources.Adherence,
				ReadinessText = Resources.Readiness,
				AnsweredCallsText = Resources.AnsweredCalls,
				AverageTalkTimeText = Resources.AverageTalkTime,
				AverageAfterWorkText = Resources.AverageAfterCallWork,
				AverageHandlingTimeText = Resources.AverageHandlingTime,
				Adherence = "86%",
				AnsweredCalls = 30 + input.OnDate.Day,
				AverageAfterWork = "15s",
				AverageHandlingTime = "135s",
				AverageTalkTime = "120s",
				Readiness = "88%",
				DisplayDate = input.OnDate.ToShortDateString()
				
			};

			var adherenceValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 88},
				 new ReportValue{Date = "2013-05-02", Value = 80},
				 new ReportValue{Date = "2013-05-03", Value = 89},
				 new ReportValue{Date = "2013-05-04", Value = 92},
				 new ReportValue{Date = "2013-05-05", Value = 75},
				 new ReportValue{Date = "2013-05-06", Value = 80},
				 new ReportValue{Date = "2013-05-07", Value = 90}

			};

			model.AdherenceValues = adherenceValues;

			var readinessValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 70},
				 new ReportValue{Date = "2013-05-02", Value = 75},
				 new ReportValue{Date = "2013-05-03", Value = 60},
				 new ReportValue{Date = "2013-05-04", Value = 33},
				 new ReportValue{Date = "2013-05-05", Value = 50},
				 new ReportValue{Date = "2013-05-06", Value = 75},
				 new ReportValue{Date = "2013-05-07", Value = 70}

			};
			model.ReadinessValues = readinessValues;

			var answeredValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 60},
				 new ReportValue{Date = "2013-05-02", Value = 95},
				 new ReportValue{Date = "2013-05-03", Value = 50},
				 new ReportValue{Date = "2013-05-04", Value = 120},
				 new ReportValue{Date = "2013-05-05", Value = 88},
				 new ReportValue{Date = "2013-05-06", Value = 75},
				 new ReportValue{Date = "2013-05-07", Value = 66}

			};

			model.AnsweredValues = answeredValues;

			var talkTimeValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 200},
				 new ReportValue{Date = "2013-05-02", Value = 180},
				 new ReportValue{Date = "2013-05-03", Value = 360},
				 new ReportValue{Date = "2013-05-04", Value = 250},
				 new ReportValue{Date = "2013-05-05", Value = 220},
				 new ReportValue{Date = "2013-05-06", Value = 260},
				 new ReportValue{Date = "2013-05-07", Value = 300}

			};

			model.TalkTimeValues = talkTimeValues;

			var aftertalkTimeValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 20},
				 new ReportValue{Date = "2013-05-02", Value = 18},
				 new ReportValue{Date = "2013-05-03", Value = 36},
				 new ReportValue{Date = "2013-05-04", Value = 25},
				 new ReportValue{Date = "2013-05-05", Value = 22},
				 new ReportValue{Date = "2013-05-06", Value = 26},
				 new ReportValue{Date = "2013-05-07", Value = 30}

			};

			model.AfterTalkTimeValues = aftertalkTimeValues;

			var handlingTimeValues = new List<ReportValue>
			{
				 new ReportValue{Date = "2013-05-01", Value = 220},
				 new ReportValue{Date = "2013-05-02", Value = 198},
				 new ReportValue{Date = "2013-05-03", Value = 366},
				 new ReportValue{Date = "2013-05-04", Value = 275},
				 new ReportValue{Date = "2013-05-05", Value = 242},
				 new ReportValue{Date = "2013-05-06", Value = 246},
				 new ReportValue{Date = "2013-05-07", Value = 330}

			};

			model.HandlingTimeValues = handlingTimeValues;
			return Json(model, JsonRequestBehavior.AllowGet);
		}
    }	
}

	//temp here only
	public class MyReportModel
	{
		public string AdherenceText { get; set; }
		public string ReadinessText { get; set; }
		public string AverageHandlingTimeText { get; set; }
		public string AverageTalkTimeText { get; set; }
		public string AverageAfterWorkText { get; set; }
		public string AnsweredCallsText { get; set; }
		public string DisplayDate { get; set; }
		public string Adherence { get; set; }
		public string Readiness { get; set; }
		public string AverageHandlingTime { get; set; }
		public string AverageTalkTime { get; set; }
		public string AverageAfterWork { get; set; }
		public int AnsweredCalls { get; set; }
		public List<ReportValue> AdherenceValues { get; set; }
		public List<ReportValue> ReadinessValues { get; set; }
		public List<ReportValue> AnsweredValues { get; set; }
		public List<ReportValue> TalkTimeValues { get; set; }
		public List<ReportValue> AfterTalkTimeValues { get; set; }
		public List<ReportValue> HandlingTimeValues { get; set; }
	}

	public class ReportValue
	{
		public string Date { get; set; }
		public int Value { get; set; }
	}

public class MyReportInput
{
	public DateTime OnDate { get; set; }
	public bool ShowWeek { get; set; }
}



