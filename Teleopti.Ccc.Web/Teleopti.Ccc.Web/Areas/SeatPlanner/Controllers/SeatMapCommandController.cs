using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	public class SeatMapCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public SeatMapCommandController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		[HttpPost]
		[UnitOfWork]
		//RobTodo: Check Permissions
		//[AddFullDayAbsencePermission]
		public virtual JsonResult AddSeatMap(AddSeatMapCommand command)
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
	
	}
}