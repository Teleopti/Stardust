using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Teleopti.Ccc.Web.Filters
{
	public abstract class AsyncFilter : FilterAttribute, IActionFilter
	{
		public async Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext,
			CancellationToken cancellationToken,
			Func<Task<HttpResponseMessage>> continuation)
		{
			await InternalActionExecuting(actionContext, cancellationToken);

			if (actionContext.Response != null)
			{
				return actionContext.Response;
			}

			HttpActionExecutedContext executedContext;

			try
			{
				var response = await continuation();
				executedContext = new HttpActionExecutedContext(actionContext, null)
				{
					Response = response
				};
			}
			catch (Exception exception)
			{
				executedContext = new HttpActionExecutedContext(actionContext, exception);
			}

			await InternalActionExecuted(executedContext, cancellationToken);
			return executedContext.Response;
		}

		public abstract Task InternalActionExecuting(HttpActionContext actionContext, CancellationToken cancellationToken);

		public abstract Task InternalActionExecuted(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken);
	}
}