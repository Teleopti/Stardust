using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakeNoPermissionProvider : IPermissionProvider
	{
		public bool HasApplicationFunctionPermission(string applicationFunctionPath) { return false; }
		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person) { return false; }
		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team) { return false; }
		public bool HasSitePermission(string applicationfunctionpath, DateOnly today, ISite site) { return false; }

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date, IAuthorizeOrganisationDetail authorizeOrganisationDetail) { return false; }
		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason)
		{
			return false;
		}
	}
}