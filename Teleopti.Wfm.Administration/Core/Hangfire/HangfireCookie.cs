using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class HangfireCookie
	{
		public static string TenantAdminRoleName = "TenantAdmin";

		// This cookie is used only for getting authenticated in Hangfire
		public static void SetHangfireAdminCookie(string userName, string email)
		{
			var claims = new List<Claim>
							{
								new Claim(ClaimTypes.Role, TenantAdminRoleName),
								new Claim(ClaimTypes.Name, userName),
								new Claim(ClaimTypes.Email, email)
							};

			var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

			HttpContext.Current.GetOwinContext().Authentication.SignIn(new AuthenticationProperties
			{
				AllowRefresh = true,
				IsPersistent = false,
				ExpiresUtc = DateTime.UtcNow.AddDays(1)
			}, identity);
		}

		public static void RemoveAdminCookie()
		{
			var identity = HttpContext.Current.GetOwinContext().Authentication.User.Identity;
			if (!identity.IsAuthenticated)
				return;

			HttpContext.Current.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
		}
	}
}