using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Controllers;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

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
	}
}