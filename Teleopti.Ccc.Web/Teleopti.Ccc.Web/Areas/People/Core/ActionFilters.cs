using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace Teleopti.Ccc.Web.Areas.People.Core
{
	public class RequireArgumentsAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(HttpActionContext actionContext)
		{
			if (actionContext.ActionArguments.Any(arg => arg.Value == null))
			{
				actionContext.Response = actionContext.Request
					.CreateErrorResponse(HttpStatusCode.BadRequest, "Invalid request data."); //TODO; Localize errormessage.
			}
		}
	}
}