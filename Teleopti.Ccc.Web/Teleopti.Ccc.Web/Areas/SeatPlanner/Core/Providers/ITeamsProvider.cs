using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.Providers
{
	public interface ITeamsProvider
	{
		IEnumerable<TeamViewModel> Get (string siteId);
		BusinessUnitWithSitesViewModel GetTeamHierarchy();
	}
}