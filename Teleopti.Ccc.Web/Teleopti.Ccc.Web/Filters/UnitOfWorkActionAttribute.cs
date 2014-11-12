using System.Web.Mvc;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
	public sealed class UnitOfWorkActionAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext filterContext)
		{
			base.OnActionExecuting(filterContext);

			var unitOfWorkFactory = DependencyResolver.Current.GetService<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
		}

		public override void OnResultExecuted(ResultExecutedContext filterContext)
		{
			base.OnResultExecuted(filterContext);

			var currentUnitOfWork = DependencyResolver.Current.GetService<ICurrentUnitOfWork>().Current();
			if (currentUnitOfWork == null) return;

			if (shouldPersist(filterContext))
			{
				currentUnitOfWork.PersistAll();
			}
			currentUnitOfWork.Dispose();
		}

		private bool shouldPersist(ResultExecutedContext filterContext)
		{
			return filterContext.Exception == null || filterContext.ExceptionHandled;
		}
	}
}