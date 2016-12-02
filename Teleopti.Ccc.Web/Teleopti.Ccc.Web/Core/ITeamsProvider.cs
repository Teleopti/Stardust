using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Core
{
	public interface ITeamsProvider
	{
		IEnumerable<TeamViewModel> Get (string siteId);
		BusinessUnitWithSitesViewModel GetTeamHierarchy();
		BusinessUnitWithSitesViewModel GetPermittedTeamHierachy(DateOnly date);
	}
}