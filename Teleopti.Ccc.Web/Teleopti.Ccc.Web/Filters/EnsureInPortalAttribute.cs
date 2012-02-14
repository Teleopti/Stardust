using System.Web.Mvc;

namespace Teleopti.Ccc.Web.Filters
{
	public class EnsureInPortalAttribute : ActionFilterAttribute
	{
		// Must Execute after UnitOfWorkAction
		public EnsureInPortalAttribute()
		{
			Order =2;
		}

		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			if (filterContext.HttpContext.Request.IsAjaxRequest()) return;

			var controllerName = filterContext.RouteData.Values["controller"];
			var actionName = filterContext.RouteData.Values["Action"];

			filterContext.Result = new RedirectResult(string.Format("~/#{0}/{1}", controllerName, actionName));

			base.OnActionExecuting(filterContext);
		}
	}
}
