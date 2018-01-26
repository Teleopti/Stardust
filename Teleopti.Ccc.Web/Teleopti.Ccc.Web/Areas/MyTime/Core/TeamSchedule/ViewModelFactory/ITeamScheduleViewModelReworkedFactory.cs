using Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.Mapping;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public interface ITeamScheduleViewModelReworkedFactory
	{
		TeamScheduleViewModelReworked GetViewModelNoReadModel(TeamScheduleViewModelData data);
	}
}
