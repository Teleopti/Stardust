using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakePermissionProvider : IPermissionProvider
	{
		public bool HasApplicationFunctionPermission(string applicationFunctionPath) { return true; }
		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person) { return true; }
		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team) { return true; }
		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IAuthorizeOrganisationDetail authorizeOrganisationDetail) { return true; }
		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason)
		{
			return true;;
		}
	}
}