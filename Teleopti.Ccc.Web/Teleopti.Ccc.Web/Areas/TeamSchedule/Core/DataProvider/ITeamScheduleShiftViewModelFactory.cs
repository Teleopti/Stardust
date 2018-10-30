using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Ccc.Web.Areas.TeamSchedule.Models;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleShiftViewModelFactory
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
		AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);

		GroupScheduleShiftViewModel MakeViewModel(IPerson person,
			DateOnly date,
			IScheduleDay scheduleDay, 
			IScheduleDay previousScheduleDay,
			bool canViewConfidential, 
			bool canViewUnpublished);

		bool IsOvertimeOnDayOff(IScheduleDay scheduleDay);
	}
}