using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonController : Controller
	{
		private readonly ISchedulePersonProvider _schedulePersonProvider;

		public PersonController(ISchedulePersonProvider schedulePersonProvider)
		{
			_schedulePersonProvider = schedulePersonProvider;
		}

		[UnitOfWorkAction,HttpGet]
		public JsonResult PeopleInGroup(DateTime date, Guid groupId)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(date), groupId,
			                                                                DefinedRaptorApplicationFunctionPaths.
			                                                                	MyTeamSchedules);
			return Json(people.Select(p => new {p.Id, p.Name.FirstName, p.Name.LastName, p.EmploymentNumber}).ToList(),
			            JsonRequestBehavior.AllowGet);
		}
	}
}