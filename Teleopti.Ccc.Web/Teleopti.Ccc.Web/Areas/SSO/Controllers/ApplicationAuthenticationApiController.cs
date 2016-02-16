using System.Web;
using System.Web.Http;
using Teleopti.Ccc.Domain.MultiTenancy;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.Infrastructure.MultiTenancy.Server.Queries;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Areas.MultiTenancy.Core;
using Teleopti.Ccc.Web.Areas.SSO.Core;
using Teleopti.Ccc.Web.Areas.SSO.Models;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.DataProvider;
using Teleopti.Ccc.Web.Areas.Start.Core.Authentication.Services;
using Teleopti.Ccc.Web.Areas.Start.Models.Authentication;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Areas.SSO.Controllers
{
	public class ApplicationAuthenticationApiController : ApiController
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

		[HttpGet, Route("SSO/ApplicationAuthenticationApi/Password")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult CheckPassword(ApplicationAuthenticationModel model)
		{
			var result = _authenticator.AuthenticateApplicationUser(model.UserName, model.Password);
			if (!result.Successful)
			{
				if (result.PasswordExpired)
				{
					return Ok(new PasswordWarningViewModel {AlreadyExpired = true});
				}
				_logLogonAttempt.SaveAuthenticateResult(model.UserName, null, false);
				
				ModelState.AddModelError("Error", result.Message);
				return BadRequest(ModelState);
			}

			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier, model.RememberMe, model.IsLogonFromBrowser);
			_logLogonAttempt.SaveAuthenticateResult(model.UserName, result.PersonId(), true);
			return Ok(new PasswordWarningViewModel { WillExpireSoon = result.HasMessage});
		}

		[HttpPut, Route("SSO/ApplicationAuthenticationApi/Password")]
		[TenantUnitOfWork]
		[NoTenantAuthentication]
		public virtual IHttpActionResult ChangePassword([FromBody]ChangePasswordInput model)
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

				ModelState.AddModelError("Error", errString);
				return BadRequest(ModelState);
			}
			_formsAuthentication.SetAuthCookie(model.UserName + TokenIdentityProvider.ApplicationIdentifier, false, false);
			return Ok(true);
		}
	}
}