using System;
using System.Net.Http.Headers;
using System.Web.Http.Filters;

namespace Teleopti.Wfm.Administration.Core
{
	public class NoCacheFilterHttp : ActionFilterAttribute
	{
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			if (actionExecutedContext.Request.Method == System.Net.Http.HttpMethod.Get && actionExecutedContext.Response != null)
			{
				actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue
				{
					NoCache = true,
					NoStore = true,
					MustRevalidate = true,
					Private = true
				};
				actionExecutedContext.Response.Headers.Pragma.Add(new NameValueHeaderValue("no-cache"));
				if (actionExecutedContext.Response.Content != null)
				{
					actionExecutedContext.Response.Content.Headers.Expires = DateTimeOffset.UtcNow;
				}
			}
			base.OnActionExecuted(actionExecutedContext);
		}
	}
}