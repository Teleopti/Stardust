using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Teleopti.Ccc.Web.Filters
{
	// Example from http://craftycodeblog.com/2010/05/15/asp-net-mvc-ajax-redirect/#comment-110 
	// MvcAjax will execute script returned (eval).

	public sealed class AjaxJavaScriptRedirectAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			base.OnActionExecuted(filterContext);
			if (!filterContext.HttpContext.Request.IsAjaxRequest()) return;

			var resultAsRedirectResult = filterContext.Result as RedirectResult;
			var resultAsRedirectToRouteResult = filterContext.Result as RedirectToRouteResult;

			if (resultAsRedirectResult != null)
			{
				var url = UrlHelper.GenerateContentUrl(resultAsRedirectResult.Url, filterContext.HttpContext);
				setResultRedirect(filterContext, url);
			}
			else if (resultAsRedirectToRouteResult != null)
			{
				var url = UrlHelper.GenerateUrl(resultAsRedirectToRouteResult.RouteName, null, null, 
							resultAsRedirectToRouteResult.RouteValues, RouteTable.Routes, filterContext.RequestContext, false);
				setResultRedirect(filterContext, url);
			}
		}

		private static void setResultRedirect(ActionExecutedContext filterContext, string url)
		{
			filterContext.Result = new JavaScriptResult
									{
										Script =
											"window.location=" +
											HttpUtility.JavaScriptStringEncode(url.Length > 0 ? url : "/", true) + ";"
									};
		}
	}
}