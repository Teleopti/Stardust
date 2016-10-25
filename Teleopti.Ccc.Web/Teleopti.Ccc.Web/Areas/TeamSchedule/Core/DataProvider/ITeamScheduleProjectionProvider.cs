using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential);
		AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);

		GroupScheduleShiftViewModel MakeViewModel(IPerson person, DateOnly date, IScheduleDay scheduleDay,
			bool canViewConfidential, bool canViewUnpublished, bool includeNote, ICommonNameDescriptionSetting agentNameSetting);

		bool IsFullDayAbsence(IScheduleDay scheduleDay);
		bool IsOvertimeOnDayOff(IScheduleDay scheduleDay);
	}
}