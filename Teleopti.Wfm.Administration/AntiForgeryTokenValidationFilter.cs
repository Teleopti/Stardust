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
			var headerToken = GetHeaderToken(actionContext);
			
			if (string.IsNullOrEmpty(headerToken) || string.IsNullOrEmpty(cookieToken) || cookieToken != headerToken) throw new HttpAntiForgeryException();
		}

		private string GetHeaderToken(HttpActionContext actionContext)
		{
			return actionContext.Request.Headers.GetValues(MvcAntiforgery.CustomHeaderName).FirstOrDefault();
		}

		private string GetCookieToken(HttpActionContext actionContext)
		{
			var cookie = actionContext.Request.Headers.GetCookies(HangfireCookie.AntiForgeryCookieName).FirstOrDefault();
			return cookie?.Cookies?.FirstOrDefault()?.Value;
		}
	}
}