using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonController : Controller
	{
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly ITeamProvider _teamProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public PersonController(ISchedulePersonProvider schedulePersonProvider, ITeamProvider teamProvider, ILoggedOnUser loggedOnUser)
		{
			_schedulePersonProvider = schedulePersonProvider;
			_teamProvider = teamProvider;
			_loggedOnUser = loggedOnUser;
		}

		[UnitOfWorkAction,HttpGet]
		public JsonResult PeopleInGroup(DateTime date, Guid groupId)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(date), groupId,
			                                                                DefinedRaptorApplicationFunctionPaths.
			                                                                	SchedulesAnywhere);
			return Json(people.Select(p => new {p.Id, p.Name.FirstName, p.Name.LastName, p.EmploymentNumber}).ToList(),
			            JsonRequestBehavior.AllowGet);
		}

		[Obsolete("seems not used anymore", true)]
		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableTeams(DateTime date)
		{
			var dateOnly = new DateOnly(date);
			var teams = _teamProvider.GetPermittedTeams(dateOnly, DefinedRaptorApplicationFunctionPaths.SchedulesAnywhere).ToList();
			if (!teams.Any())
			{
				var myTeam = _loggedOnUser.CurrentUser().MyTeam(dateOnly);
				if (myTeam != null)
					teams = new List<ITeam> {myTeam};
			}
			return Json(new { Teams = teams.Select(t => new { t.Id, t.SiteAndTeam }).OrderBy(t => t.SiteAndTeam).ToList() }, JsonRequestBehavior.AllowGet);
		}
	}
}