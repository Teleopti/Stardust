using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Common;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Infrastructure.Requests;
using Teleopti.Ccc.Infrastructure.Toggle;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Requests.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Requests;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.MyTime.Controllers
{
	public partial class OvertimeRequestsController : Controller
	{
		private readonly IOvertimeRequestPersister _overtimeRequestPersister;
		private readonly IToggleManager _toggleManager;
		private readonly ICurrentDataSource _currentDataSource;
		private readonly IAuthorization _authorization;

		public OvertimeRequestsController(IOvertimeRequestPersister overtimeRequestPersister, IToggleManager toggleManager, ICurrentDataSource currentDataSource, IAuthorization authorization)
		{
			_overtimeRequestPersister = overtimeRequestPersister;
			_toggleManager = toggleManager;
			_currentDataSource = currentDataSource;
			_authorization = authorization;
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
			var currentName = _currentDataSource.CurrentName();
			var isLicenseAvailible = DefinedLicenseDataFactory.HasLicense(currentName) &&
									 DefinedLicenseDataFactory.GetLicenseActivator(currentName).EnabledLicenseOptionPaths.Contains(
										 DefinedLicenseOptionPaths.TeleoptiCccOvertimeRequests);

			var returnVal = new OvertimeRequestLicenseAvailabilityResult
			{
				IsLicenseAvailable = isLicenseAvailible,
				HasPermissionForOvertimeRequests = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.OvertimeRequestWeb)
			};
			return Json(returnVal, JsonRequestBehavior.AllowGet);
		}
	}
}