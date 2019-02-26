using System;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Teleopti.Wfm.Administration.Core.Hangfire;

namespace Teleopti.Wfm.Administration
{
	public class AntiForgeryTokenValidationFilter : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (actionContext.ActionDescriptor.GetCustomAttributes<OverrideAuthenticationAttribute>().Any()) return;

			var cookieToken = GetCookieToken(actionContext);
			if (string.IsNullOrEmpty(cookieToken)) return;

			var headerToken = GetHeaderToken(actionContext);
			
			if (string.IsNullOrEmpty(headerToken) || cookieToken != headerToken) throw new HttpAntiForgeryException();
		}

		private string GetHeaderToken(HttpActionContext actionContext)
		{
			return actionContext.Request.Headers.GetValues(MvcAntiforgery.CustomHeaderName).FirstOrDefault();
		}

		private string GetCookieToken(HttpActionContext actionContext)
		{
			var cookie = actionContext.Request.Headers.GetCookies().SelectMany(cs =>
				cs.Cookies.Where(c => string.Equals(c.Name, HangfireCookie.AntiForgeryCookieName,
					StringComparison.InvariantCultureIgnoreCase))).FirstOrDefault();
			return cookie?.Value;
		}
	}
}