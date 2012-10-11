using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
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
			return View("WeekPartial", model);
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