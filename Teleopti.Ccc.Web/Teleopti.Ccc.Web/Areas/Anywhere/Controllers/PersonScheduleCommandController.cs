using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public PersonScheduleCommandController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		[HttpPost]
		[UnitOfWorkAction]
		[AddFullDayAbsencePermission]
		public JsonResult AddFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		[AddIntradayAbsencePermission]
		public JsonResult AddIntradayAbsence(AddIntradayAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			if (!command.IsValid())
			{
				Response.TrySkipIisCustomErrors = true;
				Response.StatusCode = 400;
				var data = new ModelStateResult { Errors = command.ValidationResult.ToArray() };
				return new JsonResult
				{
					Data = data,
					JsonRequestBehavior = JsonRequestBehavior.AllowGet
				};
			}
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		[RemoveAbsencePermission]
		public JsonResult RemovePersonAbsence(RemovePersonAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		[AddActivityPermission]
		public JsonResult AddActivity(AddActivityCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		[MoveActivityPermission]
		public JsonResult MoveActivity(MoveActivityCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}
	}
}