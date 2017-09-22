using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface ITeamScheduleProjectionForMTWProvider
	{
		ITeamScheduleProjection Projection(IScheduleDay scheduleDay);
	}
}