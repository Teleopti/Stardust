using System;
using System.Web.Mvc;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : Controller
	{
		public ActionResult AddFullDayAbsence(AddFullDayAbsenceCommand addFullDayAbsenceCommand)
		{
			return new EmptyResult();
		}
	}

	public class AddFullDayAbsenceCommand
	{
		public Guid PersonId { get; set; }
		public Guid AbsenceId { get; set; }
		public DateOnly StartDate { get; set; }
		public DateOnly EndDate { get; set; }
	}
}