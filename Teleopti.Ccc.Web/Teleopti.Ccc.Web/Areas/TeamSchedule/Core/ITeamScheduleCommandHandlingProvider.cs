using System;
using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;


namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core
{
	public interface ITeamScheduleCommandHandlingProvider
	{
		IEnumerable<Guid> CheckWriteProtectedAgents(DateOnly date, IEnumerable<Guid> agentIds);
		List<ActionResult> RemoveAbsence(RemovePersonAbsenceForm input);
		List<ActionResult> BackoutScheduleChange(BackoutScheduleChangeFormData input);
		List<ActionResult> ChangeShiftCategory(ChangeShiftCategoryFormData input);
		IList<ActionResult> MoveNonoverwritableLayers(MoveNonoverwritableLayersFormData input);
		IList<ActionResult> EditScheduleNote(EditScheduleNoteFormData input);
		IList<ActionResult> AddDayOff(AddDayOffFormData input);
		IList<ActionResult> RemoveDayOff(RemoveDayOffFormData input);
		IList<ActionResult> RemoveShift(RemoveShiftFormData input);
		IList<ActionResult> ChangeActivityType(ChangeActivityTypeFormData command);
	}
}