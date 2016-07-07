using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface IAgentScheduleViewModelReworkedMapper
	{
		AgentInTeamScheduleViewModel Map(PersonSchedule personSchedule);
		IEnumerable<AgentInTeamScheduleViewModel> Map(IEnumerable<PersonSchedule> personSchedules);

	}
}
