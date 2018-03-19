using System.Net;
using System.Web.Mvc;
using Teleopti.Ccc.Infrastructure.Foundation;
using Teleopti.Ccc.UserTexts;
using Teleopti.Ccc.Web.Core;

namespace Teleopti.Ccc.Web.Filters
{
	public class HandleOptimisticLockExceptionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuted(ActionExecutedContext filterContext)
		{
			var isAjaxRequest = filterContext.HttpContext.Request.IsAjaxRequest();
			var isExceptionNotNull = filterContext.Exception != null;
			var isExceptionNotHandled = isExceptionNotNull && !filterContext.ExceptionHandled;
			var isOptimisticLockException = isExceptionNotNull &&
											filterContext.Exception is OptimisticLockException;

			if (isAjaxRequest && isExceptionNotHandled && isOptimisticLockException)
			{
				var data = new ModelStateResult {Errors = new[] {Resources.OptimisticLockText}};
				filterContext.Result = new JsonResult
				{
					Data = data,
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
				filterContext.ExceptionHandled = true;
				filterContext.Exception = null;
				filterContext.RequestContext.HttpContext.Response.StatusCode = (int) HttpStatusCode.BadRequest;
			}
			else
			{
				base.OnActionExecuted(filterContext);
			}
		}
	}
}