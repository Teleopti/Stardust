using Teleopti.Ccc.Web.Areas.Anywhere.Core;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Core.DataProvider
{
	public interface ITeamScheduleProjectionProvider
	{
		GroupScheduleShiftViewModel Projection(IScheduleDay scheduleDay, bool canViewConfidential, ICommonNameDescriptionSetting agentNameSetting);
		AgentInTeamScheduleViewModel MakeScheduleReadModel(IPerson person, IScheduleDay scheduleDay, bool isPermittedToViewConfidential);

		bool IsFullDayAbsence(IScheduleDay scheduleDay);
		bool IsOvertimeOnDayOff(IScheduleDay scheduleDay);
	}
}