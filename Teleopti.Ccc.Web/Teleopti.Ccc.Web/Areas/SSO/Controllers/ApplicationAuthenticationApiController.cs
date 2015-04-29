using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IChangePersonPassword _changePersonPassword;

		public ApplicationAuthenticationApiController(IFormsAuthentication formsAuthentication,
																							IChangePersonPassword changePersonPassword)
		{
			_formsAuthentication = formsAuthentication;
			_changePersonPassword = changePersonPassword;
		}

		[HttpGet]
		[TenantUnitOfWork]
		public virtual JsonResult CheckPassword(ApplicationAuthenticationModel model)
		{
			var result = model.AuthenticateUser();
			if (!result.Successful)
			{
				if (result.PasswordExpired)
				{
					return Json(new PasswordWarningViewModel {AlreadyExpired = true}, JsonRequestBehavior.AllowGet);
				}
				model.SaveAuthenticateResult(result);
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", result.Message);
				return ModelState.ToJson();
			}

			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier);
			model.SaveAuthenticateResult(result);
			return Json(new PasswordWarningViewModel { WillExpireSoon = result.HasMessage}, JsonRequestBehavior.AllowGet);
		}

		[HttpPostOrPut]
		[TenantUnitOfWork]
		public virtual JsonResult ChangePassword(ChangePasswordInput model)
		{
			try
			{
				_changePersonPassword.Modify(model.UserName, model.OldPassword, model.NewPassword);
			}
			catch (HttpException httpEx)
			{
				var errString = httpEx.GetHttpCode() == 403 ? Resources.InvalidUserNameOrPassword : Resources.PasswordPolicyWarning;
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", errString);
				return ModelState.ToJson();
			}
			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier);
			return Json(true);
		}
	}
}