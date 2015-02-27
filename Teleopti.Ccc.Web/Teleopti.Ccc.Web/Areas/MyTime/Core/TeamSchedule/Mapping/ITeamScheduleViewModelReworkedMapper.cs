using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping
{
	public interface ITeamScheduleViewModelReworkedMapper
	{
		TeamScheduleViewModelReworked Map(TeamScheduleViewModelData data);
	}
}