using System;
using System.Reflection;
using System.Web;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Filters;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : ApiController
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

		[HttpPost, Route("api/PersonScheduleCommand/AddFullDayAbsence")]
		[UnitOfWork]
		[AddFullDayAbsencePermission]
		public virtual IHttpActionResult AddFullDayAbsence([FromBody]AddFullDayAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Ok(new {});
		}

		[HttpPost, Route("api/PersonScheduleCommand/AddIntradayAbsence")]
		[UnitOfWork]
		[AddIntradayAbsencePermission]
		public virtual IHttpActionResult AddIntradayAbsence([FromBody]AddIntradayAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			if (!command.IsValid())
			{
				return BadRequest(command.ValidationResult);
			}
			_commandDispatcher.Execute(command);
			return Ok(new { });
		}

		[HttpPost, Route("api/PersonScheduleCommand/RemovePersonAbsence")]
		[UnitOfWork]
		[RemoveAbsencePermission]
		public virtual IHttpActionResult RemovePersonAbsence([FromBody]MyTeamRemovePersonAbsenceCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Ok(new { });
		}

		[HttpPost, Route("api/PersonScheduleCommand/ModifyPersonAbsence")]
		[UnitOfWork]
		[RemoveAbsencePermission]
		public virtual IHttpActionResult ModifyPersonAbsence([FromBody]ModifyPersonAbsenceCommand command)
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
			return Ok(new { });
		}

		[HttpPost, Route("api/PersonScheduleCommand/AddActivity")]
		[UnitOfWork]
		[AddActivityPermission]
		public virtual IHttpActionResult AddActivity([FromBody]AddActivityCommand command)
		{
			if (command.TrackedCommandInfo != null)
				command.TrackedCommandInfo.OperatedPersonId = _loggedOnUser.CurrentUser().Id.Value;
			_commandDispatcher.Execute(command);
			return Ok(new {});
		}

		[HttpPost, Route("api/PersonScheduleCommand/MoveActivity")]
		[UnitOfWork]
		[MoveActivityPermission]
		public virtual IHttpActionResult MoveActivity([FromBody]MoveActivityCommand command)
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
			return Ok(new {});
		}

		[UnitOfWork, HttpGet, Route("api/PersonScheduleCommand/GetPersonSchedule")]
		public virtual IHttpActionResult GetPersonSchedule(Guid personId, DateTime date)
		{
			return Ok(_personScheduleDayViewModelFactory.CreateViewModel(personId, date));
		}
	}
}