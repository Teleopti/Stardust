using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationApiController : Controller
	{
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		private readonly IWebLogOn _webLogon;

		public AuthenticationApiController(IBusinessUnitsViewModelFactory businessUnitViewModelFactory, 
																					IIdentityLogon identityLogon,
																					ILogLogonAttempt logLogonAttempt,
																					IWebLogOn webLogon)
		{
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
			_webLogon = webLogon;
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult BusinessUnits()
		{
			var result = _identityLogon.LogonIdentityUser();
			if (!result.Successful)
				return errorMessage(result.Message);
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Json(businessUnits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult Logon(Guid businessUnitId)
		{
			try
			{
				var result = _identityLogon.LogonIdentityUser();
				_logLogonAttempt.SaveAuthenticateResult(string.Empty, result);
				if (!result.Successful)
					return errorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
				_webLogon.LogOn(result.DataSource.DataSourceName, businessUnitId, result.Person.Id.Value);
			}
			catch (PermissionException)
			{
				return errorMessage(Resources.InsufficientPermissionForWeb);
			}
			catch (InvalidLicenseException)
			{
				return errorMessage(Resources.TeleoptiProductActivationKeyException);
			}
			return Json(string.Empty, JsonRequestBehavior.AllowGet);
		}

		private JsonResult errorMessage(string message)
		{
			Response.StatusCode = 400;
			Response.TrySkipIisCustomErrors = true;
			ModelState.AddModelError("Error", message);
			return ModelState.ToJson();
		}
	}
}