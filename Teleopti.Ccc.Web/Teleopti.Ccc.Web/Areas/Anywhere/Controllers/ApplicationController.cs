using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Core.Startup;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	[NoCacheFilterMvc]
	public class ApplicationController : Controller
	{
		private readonly ICurrentTeleoptiPrincipal _currentTeleoptiPrincipal;

		public ApplicationController(ICurrentTeleoptiPrincipal currentTeleoptiPrincipal)
		{
			_currentTeleoptiPrincipal = currentTeleoptiPrincipal;
		}
		
		public ActionResult Redirect()
		{
			const int waitingTimeInSecond = 5;

			var principal = _currentTeleoptiPrincipal.Current();
			var currentUserName = principal.Identity.Name;
			var currentBusinessUnit = (principal.Identity as ITeleoptiIdentity)?.BusinessUnitName;
			var resAnywhereMigrated = string.Format(UserTexts.Resources.AnywhereMigrated, "<a href='./WFM'>", "</a>");
			var resPageWillBeRedirected = string.Format(UserTexts.Resources.PageWillBeRedirected,
				"<span id='pendingTime'>" + waitingTimeInSecond + "</span>");

			return new ViewResult
			{
				ViewData = new ViewDataDictionary
				{
					{"WaitingTimeInSecond", waitingTimeInSecond},
					{"CurrentUserName", currentUserName},
					{"CurrentBusinessUnit", currentBusinessUnit},
					{"ResTeamSchedule", UserTexts.Resources.Schedules},
					{"ResRta", UserTexts.Resources.RealTimeAdherence},
					{"ResReports", UserTexts.Resources.Reports},
					{"ResSignOut", UserTexts.Resources.SignOut},
					{"ResMyTeamMigrated", resAnywhereMigrated},
					{"ResPageWillBeRedirected", resPageWillBeRedirected}
				},
				ViewName = "Redirect"
			};
		}
	}
}