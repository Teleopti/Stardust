using System;
using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;
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

		[UnitOfWork,HttpGet]
		public virtual JsonResult PeopleInGroup(DateTime date, Guid groupId)
		{
			var people = _schedulePersonProvider.GetPermittedPersonsForGroup(new DateOnly(date), groupId,
			                                                                DefinedRaptorApplicationFunctionPaths.
			                                                                	MyTeamSchedules);
			return Json(people.Select(p => new {p.Id, p.Name.FirstName, p.Name.LastName, EmploymentNumber = p.EmploymentNumber}).ToList(),
			            JsonRequestBehavior.AllowGet);
		}
	}
}