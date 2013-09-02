using System.Web.Mvc;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	public class ScheduleController : Controller
	{
		private readonly IScheduleViewModelFactory _scheduleViewModelFactory;
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly INow _now;
		private readonly IOvertimeAvailabilityPersister _overtimeAvailabilityPersister;

		public ScheduleController(IScheduleViewModelFactory scheduleViewModelFactory, IRequestsViewModelFactory requestsViewModelFactory, INow now, IOvertimeAvailabilityPersister overtimeAvailabilityPersister)
		{
			_scheduleViewModelFactory = scheduleViewModelFactory;
			_requestsViewModelFactory = requestsViewModelFactory;
			_now = now;
			_overtimeAvailabilityPersister = overtimeAvailabilityPersister;
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Week()
		{
			return View("WeekPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[EnsureInPortal]
		[UnitOfWorkAction]
		public ActionResult Month()
		{
			return new ContentResult { Content = "<br/><h2>Soon...</h2>" };
		}

		[UnitOfWorkAction]
		public JsonResult FetchData(DateOnly? date)
		{
			var showForDate = date ?? _now.DateOnly();
			var model = _scheduleViewModelFactory.CreateWeekViewModel(showForDate);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpGet]
		public JsonResult FetchData2()
		{
			return Json(new {Name="Remove me"}, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Index()
		{
			return RedirectToAction("Week");
		}

		[UnitOfWorkAction]
		[HttpPost]
		public JsonResult OvertimeAvailability(OvertimeAvailabilityInput input)
		{
			return Json(_overtimeAvailabilityPersister.Persist(input), JsonRequestBehavior.AllowGet);
		}
	}
}