using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.Infrastructure.MultiTenancy.NHibernate;
using Teleopti.Ccc.Secrets.Licensing;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationApiController : Controller
	{
		private readonly IDataSourcesViewModelFactory _dataSourcesViewModelFactory;
		private readonly IBusinessUnitsViewModelFactory _businessUnitViewModelFactory;
		private readonly IWebLogOn _webLogon;

		public AuthenticationApiController(IDataSourcesViewModelFactory dataSourcesViewModelFactory, IBusinessUnitsViewModelFactory businessUnitViewModelFactory, IWebLogOn webLogon)
		{
			_dataSourcesViewModelFactory = dataSourcesViewModelFactory;
			_businessUnitViewModelFactory = businessUnitViewModelFactory;
			_webLogon = webLogon;
		}

		[HttpGet]
		public JsonResult DataSources()
		{
			return Json(_dataSourcesViewModelFactory.DataSources(), JsonRequestBehavior.AllowGet);
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult BusinessUnits(IAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
				return errorMessage(result.Message);
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Json(businessUnits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		[TenantUnitOfWork]
		public virtual JsonResult Logon(IAuthenticationModel model, Guid businessUnitId)
		{
			try
			{
				var result = model.AuthenticateUser();
				model.SaveAuthenticateResult(result);
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