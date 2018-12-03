using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.AgentInfo.Requests;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Extensions;
using Teleopti.Ccc.Web.Filters;


namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class OvertimeRequestsController : Controller
	{
		private readonly IOvertimeRequestPersister _overtimeRequestPersister;
		private readonly IToggleManager _toggleManager;
		private readonly IOvertimeRequestDefaultStartTimeProvider _overtimeRequestDefaultStartTimeProvider;

		public OvertimeRequestsController(IOvertimeRequestPersister overtimeRequestPersister, IToggleManager toggleManager, IOvertimeRequestDefaultStartTimeProvider overtimeRequestDefaultStartTimeProvider)
		{
			_overtimeRequestPersister = overtimeRequestPersister;
			_toggleManager = toggleManager;
			_overtimeRequestDefaultStartTimeProvider = overtimeRequestDefaultStartTimeProvider;
		}

		[UnitOfWork, HttpPost]
		[HandleOptimisticLockException]
		public virtual JsonResult Save(OvertimeRequestForm input)
		{
			if (!ModelState.IsValid)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return ModelState.ToJson();
			}
			try
			{
				return Json(_overtimeRequestPersister.Persist(input));
			}
			catch (InvalidOperationException e)
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				return e.ExceptionToJson(Resources.RequestCannotUpdateDelete);
			}
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult GetAvailableDays()
		{
			return Json(StaffingInfoAvailableDaysProvider.GetDays(_toggleManager), JsonRequestBehavior.AllowGet);
		}

		[UnitOfWork, HttpGet]
		public virtual JsonResult GetDefaultStartTime(DateOnly date)
		{
			var result = _overtimeRequestDefaultStartTimeProvider.GetDefaultStartTime(date);

			result.DefaultStartTimeString = result.DefaultStartTime.ToFixedDateTimeFormat();

			return Json(result, JsonRequestBehavior.AllowGet);
		}
	}
}