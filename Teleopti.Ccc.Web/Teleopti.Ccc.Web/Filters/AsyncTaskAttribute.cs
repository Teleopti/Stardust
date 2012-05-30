using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Mvc;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
	public class AsyncTaskAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			var asyncController = (AsyncController) filterContext.Controller;
			var asyncManager = asyncController.AsyncManager;
			asyncManager.Timeout = 1000*60*5;
			var culture = Thread.CurrentThread.CurrentCulture;
			var UIculture = Thread.CurrentThread.CurrentUICulture;

			asyncManager.OutstandingOperations.Increment();
			asyncManager.Parameters["task"] =
				Task.Factory.StartNew(() =>
				                      	{
				                      		try
				                      		{
				                      			Thread.CurrentThread.CurrentCulture = culture;
				                      			Thread.CurrentThread.CurrentUICulture = UIculture;

				                      			var actionName = filterContext.ActionDescriptor.ActionName + "Task";
				                      			var method = filterContext.Controller.GetType().GetMethod(actionName);
				                      			var parameters = filterContext.ActionParameters.Values.ToArray();
				                      			method.Invoke(filterContext.Controller, parameters);
				                      		}
				                      		finally
				                      		{
				                      			asyncManager.OutstandingOperations.Decrement();
				                      		}
				                      	});

		}
	}
}