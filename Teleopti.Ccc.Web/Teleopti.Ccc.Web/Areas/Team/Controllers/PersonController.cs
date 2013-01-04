using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Team.Controllers
{
	public class PersonController : Controller
	{
		private readonly ISchedulePersonProvider _schedulePersonProvider;
		private readonly ITeamProvider _teamProvider;

		public PersonController(ISchedulePersonProvider schedulePersonProvider, ITeamProvider teamProvider)
		{
			_schedulePersonProvider = schedulePersonProvider;
			_teamProvider = teamProvider;
		}

		[UnitOfWorkAction,HttpGet]
		public JsonResult PeopleInTeam(DateTime date, Guid teamId)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForTeam(new DateOnly(date), teamId,
			                                                                DefinedRaptorApplicationFunctionPaths.
			                                                                	SchedulesAdminWeb);
			return Json(people.Select(p => new {p.Id, p.Name.FirstName, p.Name.LastName, p.EmploymentNumber}).ToList(),
			            JsonRequestBehavior.AllowGet);
		}

		[UnitOfWorkAction, HttpGet]
		public JsonResult AvailableTeams(DateTime date)
		{
			var teams =
				_teamProvider.GetPermittedTeams(new DateOnly(date), DefinedRaptorApplicationFunctionPaths.SchedulesAdminWeb).Select(
					t => new {t.Id, t.SiteAndTeam}).OrderBy(t => t.SiteAndTeam).ToList();
			return Json(new
			            	{
			            		Teams = teams
			            	},JsonRequestBehavior.AllowGet);
		}
	}
}