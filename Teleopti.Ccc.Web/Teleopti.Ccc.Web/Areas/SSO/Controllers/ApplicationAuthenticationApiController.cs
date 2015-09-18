using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.NHibernate;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	[OutputCache(NoStore = true, Duration = 0, VaryByParam = "None")]
	public class ApplicationAuthenticationApiController : Controller
	{
		private readonly IFormsAuthentication _formsAuthentication;
		private readonly IChangePersonPassword _changePersonPassword;
		private readonly IApplicationUserQuery _applicationUserQuery;
		private readonly ISsoAuthenticator _authenticator;
		private readonly ILogLogonAttempt _logLogonAttempt;

		public ApplicationAuthenticationApiController(IFormsAuthentication formsAuthentication,
																							IChangePersonPassword changePersonPassword,
																							IApplicationUserQuery applicationUserQuery,
																							ISsoAuthenticator authenticator,
																							ILogLogonAttempt logLogonAttempt)
		{
			_formsAuthentication = formsAuthentication;
			_changePersonPassword = changePersonPassword;
			_applicationUserQuery = applicationUserQuery;
			_authenticator = authenticator;
			_logLogonAttempt = logLogonAttempt;
		}

		[HttpGet]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult CheckPassword(ApplicationAuthenticationModel model)
		{
			var result = _authenticator.AuthenticateApplicationUser(model.UserName, model.Password);
			if (!result.Successful)
			{
				if (result.PasswordExpired)
				{
					return Json(new PasswordWarningViewModel {AlreadyExpired = true}, JsonRequestBehavior.AllowGet);
				}
				_logLogonAttempt.SaveAuthenticateResult(model.UserName, null, false);
				Response.StatusCode = 400;
				Response.TrySkipIisCustomErrors = true;
				ModelState.AddModelError("Error", result.Message);
				return ModelState.ToJson();
			}

			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier);
			_logLogonAttempt.SaveAuthenticateResult(model.UserName, result.PersonId(), true);
			return Json(new PasswordWarningViewModel { WillExpireSoon = result.HasMessage}, JsonRequestBehavior.AllowGet);
		}

		[HttpPostOrPut]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual JsonResult ChangePassword(ChangePasswordInput model)
		{
			try
			{
				//should not have dep to tenancy here - need to rewrite client model to have the id
				var hackToGetPersonId = _applicationUserQuery.Find(model.UserName).Id;
				//
				_changePersonPassword.Modify(hackToGetPersonId, model.OldPassword, model.NewPassword);
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