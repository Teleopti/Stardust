using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;

namespace Teleopti.Ccc.Web.Areas.Start.Controllers
{
	public class StartPageController : Controller
	{
		private readonly IAuthorization _authorization;
		
		public StartPageController(IAuthorization authorization)
		{
			_authorization = authorization;
		}

		[UnitOfWork]
		public virtual RedirectResult Goto(string id)
		{
			if (id == "MyTime")
				return redirect("MyTime");
			if (id == "Anywhere")
				return redirect("Anywhere");
			if (id == "Teams")
			{
				if (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.MyTimeWeb))
					return redirect("Wfm/#/teams");
			}
			else if (id == "Rta")
				if (_authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.RealTimeAdherenceOverview))
					return redirect("Wfm/#/rta");

			return redirect("Wfm");
		}

		private RedirectResult redirect(string url)
		{
			return Redirect($"{HttpContext.Request.ApplicationPath.TrimEnd('/')}/{url}");
		}
	}
}