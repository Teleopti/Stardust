using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.Principal;


namespace Teleopti.Ccc.WebTest
{
	public class FakeNoPermissionProvider : IPermissionProvider
	{
		public bool HasApplicationFunctionPermission(string applicationFunctionPath) { return false; }
		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person) { return false; }
		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team) { return false; }
		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site) { return false; }
		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IPersonAuthorization personAuthorizationInfo) { return false; }

		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason)
		{
			return false;
		}
	}
}