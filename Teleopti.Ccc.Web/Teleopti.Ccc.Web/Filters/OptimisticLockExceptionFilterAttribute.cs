using System.Net;
using System.Net.Http;
using System.Web.Http.Filters;
using Teleopti.Ccc.Infrastructure.Foundation;

namespace Teleopti.Ccc.Web.Filters
{
	public class OptimisticLockExceptionFilterAttribute : ExceptionFilterAttribute
	{
		public override void OnException(HttpActionExecutedContext actionExecutedContext)
		{
			if (!(actionExecutedContext.Exception is OptimisticLockException)) return;
			actionExecutedContext.Response = new HttpResponseMessage(HttpStatusCode.Conflict);
		}
	}
}