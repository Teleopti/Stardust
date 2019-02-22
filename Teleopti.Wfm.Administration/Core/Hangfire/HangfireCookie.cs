using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using System.Web.Helpers;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using Teleopti.Ccc.Infrastructure.Hangfire;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireCookie : IHangfireCookie
	{
		public const string AntiForgeryCookieName = "__RequestVerificationToken_Administration";

		// This cookie is used only for getting authenticated in Hangfire
		public void SetHangfireAdminCookie(string userName, string email)
		{
			var claims = new List<Claim>
							{
								new Claim(ClaimTypes.Role, HangfireDashboardAuthorization.TenantAdminRoleName),
								new Claim(ClaimTypes.Name, userName),
								new Claim(ClaimTypes.Email, email)
							};

			var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

			var owinContext = HttpContext.Current.GetOwinContext();
			owinContext.Authentication.SignIn(new AuthenticationProperties
			{
				AllowRefresh = true,
				IsPersistent = false,
				ExpiresUtc = DateTime.UtcNow.AddDays(1)
			}, identity);
			AntiForgery.GetTokens("", out var token, out _);
			owinContext.Response.Cookies.Append(AntiForgeryCookieName, token);
		}

		public void RemoveAdminCookie()
		{
			var identity = HttpContext.Current.GetOwinContext().Authentication.User.Identity;
			if (!identity.IsAuthenticated)
				return;

			HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
		}
	}

	public interface IHangfireCookie
	{
		void SetHangfireAdminCookie(string userName, string email);
		void RemoveAdminCookie();
	}
}