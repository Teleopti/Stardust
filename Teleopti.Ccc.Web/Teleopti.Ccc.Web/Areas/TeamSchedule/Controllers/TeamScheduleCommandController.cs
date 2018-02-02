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

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/BackoutScheduleChange")]
		public virtual List<ActionResult> BackoutScheduleChangeCommand([FromBody] BackoutScheduleChangeFormData input)
		{
			return _commandHandlingProvider.BackoutScheduleChange(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/PersonWriteProtectionCheck")]
		public virtual IList<Guid> CheckPersonWriteProtection(CheckPersonWriteProtectionFormData input)
		{
			return _commandHandlingProvider.CheckWriteProtectedAgents(input.Date, input.AgentIds).ToList();
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/ChangeShiftCategory")]
		public virtual IList<ActionResult> ChangeShiftCategoryCommand([FromBody] ChangeShiftCategoryFormData input)
		{
			return _commandHandlingProvider.ChangeShiftCategory(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/MoveNonoverwritableLayers")]
		public virtual IList<ActionResult> MoveNonoverwritableLayers([FromBody] MoveNonoverwritableLayersFormData input)
		{
			return _commandHandlingProvider.MoveNonoverwritableLayers(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/EditScheduleNote")]
		public virtual IList<ActionResult> EditScheduleNote([FromBody] EditScheduleNoteFormData input)
		{
			return _commandHandlingProvider.EditScheduleNote(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamSchedule/RemoveAbsence")]
		public virtual IList<ActionResult> RemoveAbsence([FromBody] RemovePersonAbsenceForm input)
		{
			return _commandHandlingProvider.RemoveAbsence(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/AddDayOff")]
		public virtual IList<ActionResult> AddDayOff([FromBody] AddDayOffFormData input)
		{
			return _commandHandlingProvider.AddDayOff(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/RemoveDayOff")]
		public virtual IList<ActionResult> RemoveDayOff([FromBody] RemoveDayOffFormData input)
		{
			return _commandHandlingProvider.RemoveDayOff(input);
		}

		[UnitOfWork, HttpPost, Route("api/TeamScheduleCommand/RemoveShift")]
		public virtual IList<ActionResult> RemoveShift([FromBody] RemoveShiftFormData input)
		{
			return _commandHandlingProvider.RemoveShift(input);
		}

	}


}