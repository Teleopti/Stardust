using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.ViewModelFactory;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Core.Startup;

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
		public JsonResult BusinessUnits(IAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
				return ReturnErrorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
			var businessUnits = _businessUnitViewModelFactory.BusinessUnits(result.DataSource, result.Person);
			return Json(businessUnits, JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Logon(IAuthenticationModel model, Guid businessUnitId)
		{
			try
			{
				var result = model.AuthenticateUser();
				if (!result.Successful)
					return ReturnErrorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
				_webLogon.LogOn(result.DataSource.DataSourceName, businessUnitId, result.Person.Id.Value);
			}
			catch (PermissionException)
			{
				return ReturnErrorMessage(Resources.InsufficientPermissionForWeb);
			}
			return Json(null);
		}

		private JsonResult ReturnErrorMessage(string message)
		{
			Response.StatusCode = 400;
			Response.TrySkipIisCustomErrors = true;
			ModelState.AddModelError("Error", message);
			return ModelState.ToJson();
		}
	}
}