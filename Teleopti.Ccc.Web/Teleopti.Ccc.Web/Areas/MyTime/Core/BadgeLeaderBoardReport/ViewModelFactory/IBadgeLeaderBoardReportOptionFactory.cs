using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.BadgeLeaderBoardReport.ViewModelFactory
{
	public interface IBadgeLeaderBoardReportOptionFactory
	{
		dynamic CreateLeaderboardOptions(DateOnly date, string functionPath);
	}
}