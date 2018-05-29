using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public interface ITeamScheduleViewModelFactoryToggle75989Off
	{
		TeamScheduleViewModelToggle75989Off GetTeamScheduleViewModel(TeamScheduleViewModelData data);
	}

	public interface ITeamScheduleViewModelFactory
	{
		TeamScheduleViewModel GetTeamScheduleViewModel(TeamScheduleViewModelData data);
	}
}
