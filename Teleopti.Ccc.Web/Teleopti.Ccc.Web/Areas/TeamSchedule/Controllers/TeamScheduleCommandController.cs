using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers
{
	public class TeamScheduleCommandController : ApiController
	{
		private readonly ITeamScheduleCommandHandlingProvider _commandHandlingProvider;

		public TeamScheduleCommandController(ITeamScheduleCommandHandlingProvider commandHandlingProvider)
		{
			_commandHandlingProvider = commandHandlingProvider;
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/AddActivity")]
		public virtual List<FailActionResult> AddActivityCommand([FromBody]AddActivityFormData input)
		{
			return _commandHandlingProvider.AddActivity(input);
		}
		
		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/AddPersonalActivity")]
		public virtual List<FailActionResult> AddPersonalActivityCommand([FromBody]AddPersonalActivityFormData input)
		{
			return _commandHandlingProvider.AddPersonalActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/RemoveActivity")]
		public virtual List<FailActionResult> RemoveActivityCommand([FromBody]RemoveActivityFormData input)
		{
			return _commandHandlingProvider.RemoveActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/MoveActivity")]
		public virtual List<FailActionResult> MoveActivityCommand([FromBody] MoveActivityFormData input)
		{
			return _commandHandlingProvider.MoveActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/BackoutScheduleChange")]
		public virtual List<FailActionResult> BackoutScheduleChangeCommand([FromBody] BackoutScheduleChangeFormData input)
		{
			return _commandHandlingProvider.BackoutScheduleChange(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/PersonWriteProtectionCheck")]
		public virtual IList<Guid> CheckPersonWriteProtection(CheckPersonWriteProtectionFormData input)
		{
			return _commandHandlingProvider.CheckWriteProtectedAgents(input.Date, input.AgentIds).ToList();
		}
	}
}