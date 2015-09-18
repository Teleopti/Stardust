using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Core;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationApiController : Controller
	{
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;
		private readonly IIdentityLogon _identityLogon;
		private readonly ILogLogonAttempt _logLogonAttempt;
		private readonly IWebLogOn _webLogon;
		private readonly IDataSourceForTenant _dataSourceForTenant;

		public AuthenticationApiController(IBusinessUnitsViewModelFactory businessUnitViewModelFactory, 
																					IIdentityLogon identityLogon,
																					ILogLogonAttempt logLogonAttempt,
																					IWebLogOn webLogon,
																					IDataSourceForTenant dataSourceForTenant)
		{
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
			_identityLogon = identityLogon;
			_logLogonAttempt = logLogonAttempt;
			_webLogon = webLogon;
			_dataSourceForTenant = dataSourceForTenant;
		}

		[HttpGet]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult BusinessUnits()
		{
			var result = _identityLogon.LogonIdentityUser();
			if (!result.Successful)
				return errorMessage("Unknown error");
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Json(businessUnits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult Logon(Guid businessUnitId)
		{
			var result = _identityLogon.LogonIdentityUser();
			_logLogonAttempt.SaveAuthenticateResult(string.Empty, result.PersonId(), result.Successful);
			if (!result.Successful)
				return errorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
			try
			{
				_webLogon.LogOn(result.DataSource.DataSourceName, businessUnitId, result.Person.Id.Value, result.TenantPassword);
			}
			catch (PermissionException)
			{
				return errorMessage(Resources.InsufficientPermissionForWeb);
			}
			catch (InvalidLicenseException)
			{
				_dataSourceForTenant.RemoveDataSource(result.DataSource.DataSourceName);
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