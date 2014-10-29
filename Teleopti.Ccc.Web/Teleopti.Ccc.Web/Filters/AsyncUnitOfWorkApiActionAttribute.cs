using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Mvc;
using Teleopti.Interfaces.Infrastructure;

namespace Teleopti.Ccc.Web.Filters
{
	public class AsyncUnitOfWorkApiActionAttribute : AsyncFilter
	{
		public override Task InternalActionExecuted(HttpActionExecutedContext actionExecutedContext,
			CancellationToken cancellationToken)
		{
			var result = Task.FromResult(true);
			var currentUnitOfWork = DependencyResolver.Current.GetService<ICurrentUnitOfWork>().Current();
			if (currentUnitOfWork == null) return result;


			if (actionExecutedContext.Exception != null)
			{
				currentUnitOfWork.Dispose();
			}
			else
			{
				currentUnitOfWork.PersistAll();
				currentUnitOfWork.Dispose();
			}
			return result;
		}

		public override Task InternalActionExecuting(HttpActionContext actionContext, CancellationToken cancellationToken)
		{
			var unitOfWorkFactory = DependencyResolver.Current.GetService<ICurrentUnitOfWorkFactory>();
			unitOfWorkFactory.LoggedOnUnitOfWorkFactory().CreateAndOpenUnitOfWork();
			return Task.FromResult(true);
		}
	}
}