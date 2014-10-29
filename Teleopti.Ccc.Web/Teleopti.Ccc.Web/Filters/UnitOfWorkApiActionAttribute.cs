using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class UnitOfWorkApiActionAttribute : System.Web.Http.Filters.ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			base.OnActionExecuting(actionContext);

			var unitOfWorkFactory = DependencyResolver.Current.GetService<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
		}

		public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
		{
			base.OnActionExecuted(actionExecutedContext);
			var currentUnitOfWork = DependencyResolver.Current.GetService<ICurrentUnitOfWork>().Current();
			if (actionExecutedContext.Exception != null)
			{
				currentUnitOfWork.Dispose();
				return;
			}
			if (currentUnitOfWork != null)
			{
				currentUnitOfWork.PersistAll();
				currentUnitOfWork.Dispose();
			}
		}
	}
}