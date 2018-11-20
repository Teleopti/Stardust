using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Filters;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.MyTime.Core.WeekSchedule.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Schedule.WeekSchedule;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.MyTimeWeb)]
	public class ScheduleController : Controller
	{
		private readonly IRequestsViewModelFactory _requestsViewModelFactory;
		private readonly IOvertimeAvailabilityPersister _overtimeAvailabilityPersister;
		private readonly IAbsenceReportPersister _absenceReportPersister;

		public ScheduleController(IRequestsViewModelFactory requestsViewModelFactory, IOvertimeAvailabilityPersister overtimeAvailabilityPersister, IAbsenceReportPersister absenceReportPersister)
		{
			_requestsViewModelFactory = requestsViewModelFactory;
			_overtimeAvailabilityPersister = overtimeAvailabilityPersister;
			_absenceReportPersister = absenceReportPersister;
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ActionResult Week()
		{
			return View("WeekPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[HttpGet]
		[EnsureInPortal]
		[UnitOfWork]
		public virtual ActionResult MobileDay()
		{
			return View("MobileDayPartial", _requestsViewModelFactory.CreatePageViewModel());
		}

		[HttpGet]
		[EnsureInPortal]
		public virtual ActionResult Month()
		{
			return View("MonthPartial");
		}

		[HttpGet]
		[EnsureInPortal]
		public virtual ActionResult MobileMonth()
		{
			return View("MobileMonthPartial");
		}

		[HttpGet]
		public ActionResult Index()
		{
			return RedirectToAction("Week");
		}

		[UnitOfWork]
		[HttpPost]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb)]
		public virtual JsonResult OvertimeAvailability(OvertimeAvailabilityInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			return Json(_overtimeAvailabilityPersister.Persist(input), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork]
		[HttpPost]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.AbsenceReport)]
		public virtual JsonResult ReportAbsence(AbsenceReportInput input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			try
			{
				return Json(_absenceReportPersister.Persist(input), JsonRequestBehavior.AllowGet);
			}
			catch (InvalidOperationException e)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return e.ExceptionToJson(Resources.AbsenceCanNotBeReported);
			}
		}

		[UnitOfWork]
		[HttpDelete]
		[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OvertimeAvailabilityWeb)]
		public virtual JsonResult DeleteOvertimeAvailability(DateOnly date)
		{
			return Json(_overtimeAvailabilityPersister.Delete(date));
		}
	}
}