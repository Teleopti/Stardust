using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.HealthCheck.Controllers
{
	[ApplicationFunction(DefinedRaptorApplicationFunctionPaths.OpenPermissionPage)]
	public class ApplicationController : Controller
	{
		public ViewResult Index()
		{
			return new ViewResult();
		}
	}
}
