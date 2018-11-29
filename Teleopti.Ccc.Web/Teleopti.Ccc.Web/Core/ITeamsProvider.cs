using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels;


namespace Teleopti.Ccc.Web.Core
{
	public interface ITeamsProvider
	{
		BusinessUnitWithSitesViewModel GetTeamHierarchy();
		BusinessUnitWithSitesViewModel GetPermittedTeamHierachy(DateOnly date, string permission);
		BusinessUnitWithSitesViewModel GetOrganizationWithPeriod(DateOnlyPeriod dateOnlyPeriod, string functionPath);
		BusinessUnitWithSitesViewModel GetOrganizationBasedOnRawData(DateOnlyPeriod dateOnlyPeriod, string functionPath);
	}
}