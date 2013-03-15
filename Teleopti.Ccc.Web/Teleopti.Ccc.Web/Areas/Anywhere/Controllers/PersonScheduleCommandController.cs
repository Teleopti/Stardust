using System;
using System.Web.Mvc;
using Teleopti.Ccc.Domain.ApplicationLayer;
using Teleopti.Ccc.Web.Areas.MyTime.Core;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Controllers
{
	public class PersonScheduleCommandController : Controller
	{
		private readonly ICommandDispatcher _commandDispatcher;

		public PersonScheduleCommandController(ICommandDispatcher commandDispatcher)
		{
			_commandDispatcher = commandDispatcher;
		}

		[HttpPostOrPut]
		public void AddFullDayAbsence(AddFullDayAbsenceCommand command)
		{
			_commandDispatcher.Execute(command);
		}
	}
}