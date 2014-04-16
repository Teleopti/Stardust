using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Common.Time;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Models.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
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
		public ActionResult MobileWeek()
		{
			return View("WeekPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

        [EnsureInPortal]
        public ActionResult Month()
        {
            return View("MonthPartial");
        }

		[UnitOfWorkAction]
		public JsonResult FetchData(DateOnly? date)
		{
			var showForDate = date ?? _now.LocalDateOnly();
			var model = _scheduleViewModelFactory.CreateWeekViewModel(showForDate);
			return Json(model, JsonRequestBehavior.AllowGet);
		}

        [UnitOfWorkAction]
        public JsonResult FetchMonthData(DateOnly? date)
        {
            var showForDate = date ?? _now.LocalDateOnly();
            var model = _scheduleViewModelFactory.CreateMonthViewModel(showForDate);
            return Json(model, JsonRequestBehavior.AllowGet);
        }

		public ActionResult Index()
		{
			return RedirectToAction("Week");
		}

		[UnitOfWorkAction]
		[HttpPost]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb)]
		public JsonResult OvertimeAvailability(OvertimeAvailabilityInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_overtimeAvailabilityPersister.Persist(input), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction]
		[HttpDelete]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb)]
		public JsonResult DeleteOvertimeAvailability(DateOnly date)
		{
			return Json(_overtimeAvailabilityPersister.Delete(date));
		}
	}
}