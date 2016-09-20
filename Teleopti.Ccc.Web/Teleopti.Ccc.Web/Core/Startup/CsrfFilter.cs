using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class CsrfFilterHttp : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			base.OnActionExecuting(actionContext);

			var referrer = actionContext.Request.Headers.Referrer;
			IEnumerable<string> foundValues;
			if (actionContext.Request.Headers.TryGetValues("Origin", out foundValues))
			{
				var firstOrigin = foundValues.FirstOrDefault();
				if (!string.IsNullOrEmpty(firstOrigin))
				{
					referrer = new Uri(firstOrigin);
				}
			}
			if (referrer != null && referrer.Authority != actionContext.Request.RequestUri.Authority)
			{
				actionContext.Response.StatusCode = HttpStatusCode.Forbidden;
			}
		}
	}

	public class CsrfFilter : System.Web.Mvc.ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);
			if (filterContext.HttpContext.Request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase) ||
				filterContext.HttpContext.Request.HttpMethod.Equals("OPTIONS", StringComparison.OrdinalIgnoreCase)) return;

			var referer = filterContext.HttpContext.Request.Headers["Origin"] ??
						  filterContext.HttpContext.Request.Headers["Referer"];
 
			var url = filterContext.HttpContext.Request.Url;
			if (!string.IsNullOrEmpty(referer) && url!=null)
			{
				var refererUri = new Uri(referer);
				if (refererUri.Host != url.Host)
				{
					filterContext.Result = new HttpStatusCodeResult(403);
					filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
				}
			}
		}
	}

}