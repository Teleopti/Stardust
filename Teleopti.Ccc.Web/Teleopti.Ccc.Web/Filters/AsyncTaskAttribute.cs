using System;
using System.Diagnostics;
using System.Linq;
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
			var unitOfWorkFactory = DependencyResolver.Current.GetService<IUnitOfWorkFactory>();

			asyncManager.OutstandingOperations.Increment();
			Task.Factory.StartNew(() =>
			                      	{
			                      		try
			                      		{
			                      			using (var unitOfWork = unitOfWorkFactory.CreateAndOpenUnitOfWork())
			                      			{
			                      				var actionName = filterContext.ActionDescriptor.ActionName + "Task";
			                      				var method = filterContext.Controller.GetType().GetMethod(actionName);
			                      				var parameters = filterContext.ActionParameters.Values.ToArray();
			                      				method.Invoke(filterContext.Controller, parameters);
			                      				unitOfWork.PersistAll();
			                      			}
			                      		}
			                      		catch (Exception e)
			                      		{
			                      			asyncManager.Parameters["exception"] = e;
			                      		}
			                      		asyncManager.OutstandingOperations.Decrement();
			                      	});

		}
	}
}