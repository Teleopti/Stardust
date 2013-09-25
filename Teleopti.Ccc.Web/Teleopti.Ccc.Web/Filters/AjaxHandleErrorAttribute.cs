using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Models.Shared;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class AjaxHandleErrorAttribute : HandleErrorAttribute
	{
		private readonly IErrorMessageProvider _errorMessageProvider;

		// constructor dependencies can also be resolved using properties if this ever is needed to be used as an attribute
		public AjaxHandleErrorAttribute(IErrorMessageProvider errorMessageProvider)
		{
			_errorMessageProvider = errorMessageProvider;
			Order = 1;
		}

		public override void OnException(ExceptionContext filterContext)
		{
			base.OnException(filterContext);

			if (filterContext.HttpContext.Request.AcceptsJson())
			{
				if (filterContext.ExceptionHandled)
				{
					MakeJsonResult(filterContext);
				}
				else if (filterContext.HttpContext.IsCustomErrorEnabled && filterContext.Exception is HttpException)
				{
					MakeJsonResult(filterContext);
					filterContext.ExceptionHandled = true;
					filterContext.HttpContext.Response.Clear();
					filterContext.HttpContext.Response.StatusCode = ((HttpException)filterContext.Exception).GetHttpCode();
					filterContext.HttpContext.Response.TrySkipIisCustomErrors = true;
				}
			}
			else
			{
				if (filterContext.ExceptionHandled)
				{
					ModifyViewResult(filterContext);
				}
			}
		}

		private void MakeJsonResult(ExceptionContext filterContext)
		{
			var controllerName = filterContext.RouteData.Values["controller"] as string;
			var actionName = filterContext.RouteData.Values["action"] as string;
			var handleErrorInfo = new HandleErrorInfo(filterContext.Exception, controllerName, actionName);
			filterContext.Result = new JsonResult
			                       	{
			                       		Data = MakeErrorViewModel(handleErrorInfo),
			                       		ContentType = "application/json",
										JsonRequestBehavior = JsonRequestBehavior.AllowGet
			                       	};
		}

		private void ModifyViewResult(ExceptionContext filterContext)
		{
			var viewResult = filterContext.Result as ViewResult;
			var handleErrorInfo = viewResult.Model as HandleErrorInfo;

			filterContext.HttpContext.Response.StatusCode = 200;

			if (filterContext.HttpContext.Request.IsAjaxRequest())
				viewResult.ViewName += "Partial";
			viewResult.ViewData = new ViewDataDictionary<ErrorViewModel>(MakeErrorViewModel(handleErrorInfo));
		}

		private ErrorViewModel MakeErrorViewModel(HandleErrorInfo handleErrorInfo)
		{
			return new ErrorViewModel
			       	{
			       		Message = _errorMessageProvider.ResolveMessage(handleErrorInfo),
			       		ShortMessage = _errorMessageProvider.ResolveShortMessage(handleErrorInfo)
			       	};
		}
	}
}