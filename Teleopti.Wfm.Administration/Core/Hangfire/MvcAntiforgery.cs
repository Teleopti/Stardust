using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using Hangfire.Dashboard.Owin;

namespace Teleopti.Wfm.Administration.Core.Hangfire
{
	public class MvcAntiforgery : IOwinDashboardAntiforgery
	{
		public MvcAntiforgery()
		{
			AntiForgeryConfig.UniqueClaimTypeIdentifier = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
		}

		public const string CustomHeaderName = "X-XSRF-TOKEN";
		public string HeaderName => CustomHeaderName;


		public string GetToken(IDictionary<string, object> environment)
		{
			AntiForgery.GetTokens(GetCookieToken(), out _, out var formToken);

			return formToken;
		}

		public bool ValidateRequest(IDictionary<string, object> environment)
		{
			try
			{
				if (HttpContext.Current.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
				{
					AntiForgery.Validate(GetCookieToken(), GetHeaderToken());
				}

				return true;
			}
			catch (HttpAntiForgeryException)
			{
				return false;
			}
		}

		private string GetHeaderToken()
		{
			return HttpContext.Current.Request.Headers[HeaderName];
		}

		private string GetCookieToken()
		{
			var cookie = HttpContext.Current.Request.Cookies[HangfireCookie.AntiForgeryCookieName];
			return cookie != null && !String.IsNullOrEmpty(cookie.Value) ? cookie.Value : null;
		}
	}
}