using System.Threading;
using System.Web.Http;
using System.Web.Http.Controllers;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Filters
{
	public class AuthorizeTeleoptiAttribute : AuthorizeAttribute
	{
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			return Thread.CurrentPrincipal is ITeleoptiPrincipal && base.IsAuthorized(actionContext);
		}
	}
}