using System.Collections.Generic;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ITimeLineViewModelMapperToggle75989Off
	{
		TeamScheduleTimeLineViewModelToggle75989Off[] Map(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules, DateOnly date);
	}
}