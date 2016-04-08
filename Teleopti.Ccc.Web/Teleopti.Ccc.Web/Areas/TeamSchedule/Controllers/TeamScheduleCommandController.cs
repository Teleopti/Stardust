using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Teleopti.Ccc.Domain.Aop;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Core;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

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
		public virtual void AddActivityCommand([FromBody]AddActivityFormData input)
		{
			_commandHandlingProvider.AddActivity(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/PersonWriteProtectionCheck")]
		public virtual IList<Guid> CheckPersonWriteProtection(DateOnly date, IEnumerable<Guid> agentIds)
		{
			return _commandHandlingProvider.CheckWriteProtectedAgents(date, agentIds).ToList();
		}
	}
}