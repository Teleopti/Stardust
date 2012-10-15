using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class killme
	{
		public bool AbsenceRequestPermission { get; set; }
		public IEnumerable<AbsenceTypeViewModel> AbsenceTypes { get; set; }
	}

	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	public class ScheduleController : Controller
	{
		private readonly IScheduleViewModelFactory _scheduleViewModelFactory;
		private readonly INow _now;

		public ScheduleController(IScheduleViewModelFactory scheduleViewModelFactory, INow now)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
			_now = now;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Week(DateOnly? dateSelection)
		{
			var showForDate = dateSelection ?? _now.DateOnly();

			var model = _scheduleViewModelFactory.CreateWeekViewModel(showForDate);

			if (AcceptsJson())
				return Json(model, JsonRequestBehavior.AllowGet);

			var model2 = new killme();
			model2.AbsenceRequestPermission = true;
			model2.AbsenceTypes = new List<AbsenceTypeViewModel>{new AbsenceTypeViewModel{Id=Guid.NewGuid(), Name = "roger"}};
			return View("WeekPartial", model2);
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Month()
		{
			return new ContentResult { Content = "<br/><h2>Soon...</h2>" };
		}

		[UnitOfWorkAction]
		public JsonResult Bajs(DateOnly? date)
		{
			var showForDate = date ?? _now.DateOnly();
			var model = _scheduleViewModelFactory.CreateWeekViewModel(showForDate);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Index()
		{
			return RedirectToAction("Week");
		}

		private bool AcceptsJson()
		{
			return HttpContext != null && HttpContext.Request.AcceptsJson();
		}
	}
}