using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;

		public PersonScheduleCommandController(ICommandDispatcher commandDispatcher)
		{
			_commandDispatcher = commandDispatcher;
		}

		[HttpPost]
		[UnitOfWorkAction]
		public JsonResult AddFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		public JsonResult AddIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		public JsonResult RemovePersonAbsence(RemovePersonAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		public JsonResult AddActivity(AddActivityCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}
	}
}