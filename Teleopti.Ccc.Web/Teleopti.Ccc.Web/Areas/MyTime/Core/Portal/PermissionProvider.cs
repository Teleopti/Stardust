using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public class PermissionProvider : IPermissionProvider
	{
		private readonly IPrincipalAuthorization _principalAuthorization;

		public PermissionProvider(IPrincipalAuthorization principalAuthorization)
		{
			_principalAuthorization = principalAuthorization;
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath, date, person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return _principalAuthorization.IsPermitted(applicationFunctionPath, date, team);
		}
	}
}