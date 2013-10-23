using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface IDefaultTeamProvider
	{
		ITeam DefaultTeam(DateOnly date);
	}
}