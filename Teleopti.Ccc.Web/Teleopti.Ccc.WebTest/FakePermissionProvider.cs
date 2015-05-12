using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.WebTest
{
	public class FakePermissionProvider : IPermissionProvider
	{
		public bool? hasApplicationFunctionPermission { get; set; }
		public bool? hasPersonPermission{ get; set; }
		public bool? hasTeamPermission { get; set; }
		public bool? hasOrganisationDetailPermission { get; set; }
		public bool? isPersonSchedulePublished { get; set; }

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			if (hasApplicationFunctionPermission != null) return (bool) hasApplicationFunctionPermission;
			return true;
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			if (hasPersonPermission != null) return (bool) hasPersonPermission;
			return true;
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			if (hasTeamPermission != null) return (bool) hasTeamPermission;
			return true;
		}

		public bool HasOrganisationDetailPermission(string applicationFunctionPath, DateOnly date,
			IAuthorizeOrganisationDetail authorizeOrganisationDetail)
		{

			if (hasOrganisationDetailPermission != null) return (bool) hasOrganisationDetailPermission;
			return true;
		}
		public bool IsPersonSchedulePublished(DateOnly date, IPerson person, ScheduleVisibleReasons reason)
		{
			if (isPersonSchedulePublished != null) return (bool) isPersonSchedulePublished;
			return true;;
		}
	}
}