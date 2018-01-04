using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer.OvertimeRequests;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public class OvertimeRequestsController : Controller
	{
		private readonly IOvertimeRequestPersister _overtimeRequestPersister;
		private readonly IToggleManager _toggleManager;
		private readonly IOvertimeRequestAvailability _overtimeRequestLicense;

		public OvertimeRequestsController(IOvertimeRequestPersister overtimeRequestPersister, IToggleManager toggleManager, IOvertimeRequestAvailability overtimeRequestLicense)
		{
			_overtimeRequestPersister = overtimeRequestPersister;
			_toggleManager = toggleManager;
			_overtimeRequestLicense = overtimeRequestLicense;
		}

		[UnitOfWork, HttpPost]
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
		public virtual JsonResult GetLicenseAvailability()
		{
			return Json(_overtimeRequestLicense.IsEnabled(), JsonRequestBehavior.AllowGet);
		}
	}
}