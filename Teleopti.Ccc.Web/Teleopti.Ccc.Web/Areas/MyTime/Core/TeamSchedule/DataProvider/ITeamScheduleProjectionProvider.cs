using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamScheduleProjectionProvider
	{
		ITeamScheduleProjection Projection(IScheduleDay scheduleDay);
	}
}