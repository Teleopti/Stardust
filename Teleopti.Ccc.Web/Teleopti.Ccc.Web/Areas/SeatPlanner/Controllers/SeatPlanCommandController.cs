using System;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Domain.ApplicationLayer.Commands;
using System.Web.Helpers;
using Teleopti.Ccc.Web.Core.Aop.Aspects;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Controllers
{
	public class SeatPlanCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;
		private readonly ILoggedOnUser _loggedOnUser;

		public SeatPlanCommandController(ICommandDispatcher commandDispatcher, ILoggedOnUser loggedOnUser)
		{
			_commandDispatcher = commandDispatcher;
			_loggedOnUser = loggedOnUser;
		}

		[HttpPost]
		[UnitOfWork]
		//RobTodo: Check Permissions
		//[AddFullDayAbsencePermission]
		public virtual JsonResult AddSeatPlan(AddSeatPlanCommand command)
		{
			//Robtodo: temporary throw away code just for the prototype..
			loadLocationsFromFile(command);

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

		private void loadLocationsFromFile (AddSeatPlanCommand command)
		{
			String path = Request.MapPath ("~/Areas/SeatPlanner/Content/Temp/Locations.txt");
			var locationString = System.IO.File.ReadAllText (path);
			if (!String.IsNullOrEmpty (locationString))
			{
				var locationData = System.Web.Helpers.Json.Decode (locationString);
				command.LocationsFromFile = locationData;
			}
		}
	}
}