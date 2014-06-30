using System.Linq;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Core;
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
		[AddFullDayAbsencePermission]
		public JsonResult AddFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		[AddIntradayAbsencePermission]
		public JsonResult AddIntradayAbsence(AddIntradayAbsenceCommand command)
		{
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
		public JsonResult AddActivity(AddActivityCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWorkAction]
		public JsonResult MoveActivity(MoveActivityCommand command)
		{
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}
	}
}