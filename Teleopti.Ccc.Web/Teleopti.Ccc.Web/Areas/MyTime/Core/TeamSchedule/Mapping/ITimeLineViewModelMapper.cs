using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ITimeLineViewModelMapper
	{
		TeamScheduleTimeLineViewModel[] Map(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules, DateOnly date);
	}
	public interface ITimeLineViewModelMapperToggle75989Off
	{
		TeamScheduleTimeLineViewModelToggle75989Off[] Map(IEnumerable<AgentInTeamScheduleViewModel> agentSchedules, DateOnly date);
	}
}