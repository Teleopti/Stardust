using System;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using log4net;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Core;
using Teleopti.Ccc.Web.Filters;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPersonScheduleDayViewModelFactory _personScheduleDayViewModelFactory;

		public PersonScheduleCommandController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser, IPersonScheduleDayViewModelFactory personScheduleDayViewModelFactory)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
			_personScheduleDayViewModelFactory = personScheduleDayViewModelFactory;
		}

		[HttpPost]
		[UnitOfWork]
		[AddFullDayAbsencePermission]
		public virtual JsonResult AddFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWork]
		[AddIntradayAbsencePermission]
		public virtual JsonResult AddIntradayAbsence(AddIntradayAbsenceCommand command)
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
		[UnitOfWork]
		[RemoveAbsencePermission]
		public virtual JsonResult RemovePersonAbsence(RemovePersonAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWork]
		[RemoveAbsencePermission]
		public virtual JsonResult ModifyPersonAbsence(ModifyPersonAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			try
			{
				_commandDispatcher.Execute(command);
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException is ArgumentException)
					throw new HttpException(501, e.InnerException.Message);
			}
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWork]
		[AddActivityPermission]
		public virtual JsonResult AddActivity(AddActivityCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[HttpPost]
		[UnitOfWork]
		[MoveActivityPermission]
		public virtual JsonResult MoveActivity(MoveActivityCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			try
			{
				_commandDispatcher.Execute(command);
			}
			catch (TargetInvocationException e)
			{
				if (e.InnerException is ArgumentException)
					throw new HttpException(501, e.InnerException.Message);
			}
			return Json(new object(), JsonRequestBehavior.DenyGet);
		}

		[UnitOfWorkAction, HttpGet]
		public virtual JsonResult GetPersonSchedule(Guid personId, DateTime date)
		{
			return Json(_personScheduleDayViewModelFactory.CreateViewModel(personId, date), JsonRequestBehavior.AllowGet);
		}
	}
}