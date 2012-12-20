using System.Threading;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.Team.Controllers
{
	public class ApplicationController : Controller
	{
		private readonly IPrincipalAuthorization _principalAuthorization;
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;
		
		public ApplicationController(IPrincipalAuthorization principalAuthorization, ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_principalAuthorization = principalAuthorization;
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}

		public FilePathResult Index()
		{
			return new FilePathResult("~/Areas/Team/Content/Templates/index.html", "text/html");
		}

		[HttpGet,OutputCache(NoStore = true,Duration = 0)]
		public JsonResult NavigationContent()
		{
			return Json(new
			            	{
			            		UserName = _currentTeleoptiPrincipal.Current().Identity.Name,
								IsMyTimeAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb),
								IsMobileReportsAvailable = _principalAuthorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MobileReports),
			            	}, JsonRequestBehavior.AllowGet);
		}
	}
}