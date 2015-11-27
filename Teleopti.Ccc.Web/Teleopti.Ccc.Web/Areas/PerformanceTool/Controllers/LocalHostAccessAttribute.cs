using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;

namespace Teleopti.Ccc.Web.Areas.PerformanceTool.Controllers
{
	public class LocalHostAccessAttribute : AuthorizeAttribute
	{
		protected override bool IsAuthorized(HttpActionContext actionContext)
		{
			if (actionContext.ControllerContext.RequestContext.IsLocal)
				return true;
			throw new HttpException(404, "");
		}
	}
}