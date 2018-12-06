using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Http.Filters;
using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Core.Startup
{
	public class NoCacheFilterHttp : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			if (actionExecutedContext.Request.Method == System.Net.Http.HttpMethod.Get && actionExecutedContext.Response != null)
			{
				if (!(actionExecutedContext.ActionContext?.ActionDescriptor?.GetFilters()?
					.OfType<CacheFilterHttpAttribute>().Any() ?? false))
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
			}
			base.OnActionExecuted(actionExecutedContext);
		}
	}

	public class CacheFilterHttpAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		private readonly bool _isPrivate;

		public CacheFilterHttpAttribute(bool IsPrivate = false)
		{
			_isPrivate = IsPrivate;
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			if (actionExecutedContext.Request.Method == System.Net.Http.HttpMethod.Get && actionExecutedContext.Response != null)
			{
				actionExecutedContext.Response.Headers.CacheControl = new CacheControlHeaderValue
				{
					Public = !_isPrivate,
					Private = _isPrivate,
					MaxAge = TimeSpan.FromMinutes(30)
				};
			}
			base.OnActionExecuted(actionExecutedContext);
		}
	}

	public class NoCacheFilterMvc : System.Web.Mvc.ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var cache = filterContext.HttpContext.Response.Cache;
			cache.SetCacheability(HttpCacheability.NoCache);
			cache.SetRevalidation(HttpCacheRevalidation.AllCaches);
			cache.SetExpires(DateTime.UtcNow);
			cache.AppendCacheExtension("private, no-store");
			cache.SetProxyMaxAge(TimeSpan.Zero);
			
			base.OnActionExecuted(filterContext);
		}
	}
}