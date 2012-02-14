using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0)]
	public class ScheduleController : Controller
	{
		private readonly IScheduleViewModelFactory _scheduleViewModelFactory;

		public ScheduleController(IScheduleViewModelFactory scheduleViewModelFactory)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Week(DateOnly? dateSelection)
		{
			var showForDate = dateSelection ?? DateOnly.Today;

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