using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;
using IDataSourcesViewModelFactory = Teleopti.Ccc.Web.Areas.SSO.Core.IDataSourcesViewModelFactory;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class AuthenticationApiController : Controller
	{
		private readonly IDataSourcesViewModelFactory _dataSourcesViewModelFactory;

		public AuthenticationApiController(IDataSourcesViewModelFactory dataSourcesViewModelFactory)
		{
			_dataSourcesViewModelFactory = dataSourcesViewModelFactory;
		}

		[HttpGet]
		public JsonResult DataSources()
		{
			return Json(_dataSourcesViewModelFactory.DataSources(), JsonRequestBehavior.AllowGet);
		}

		[HttpPost]
		public JsonResult Logon(IAuthenticationModel model)
		{
			try
			{
				var result = model.AuthenticateUser();
				model.SaveAuthenticateResult(result);
				if (!result.Successful)
					return errorMessage(Resources.LogOnFailedInvalidUserNameOrPassword);
			}
			catch (PermissionException)
			{
				return errorMessage(Resources.InsufficientPermissionForWeb);
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