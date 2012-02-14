using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public interface IDefaultTeamCalculator
	{
		ITeam Calculate(DateOnly date);
	}
}