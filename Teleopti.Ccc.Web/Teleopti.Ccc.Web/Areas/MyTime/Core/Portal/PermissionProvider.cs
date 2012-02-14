using System.Threading;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public class PermissionProvider : IPermissionProvider
	{
		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			var principal = (TeleoptiPrincipal) Thread.CurrentPrincipal;
			return principal.PrincipalAuthorization.IsPermitted(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			var principal = (TeleoptiPrincipal)Thread.CurrentPrincipal;
			return principal.PrincipalAuthorization.IsPermitted(applicationFunctionPath, date, person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			var principal = (TeleoptiPrincipal)Thread.CurrentPrincipal;
			return principal.PrincipalAuthorization.IsPermitted(applicationFunctionPath, date, team);
		}
	}
}