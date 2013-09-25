using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Anywhere.Core
{
	public interface ITeamScheduleViewModelMapper
	{
		IEnumerable<TeamScheduleShiftViewModel> Map(TeamScheduleData data);
	}
}