using System.Collections.Generic;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Portal
{
	public class PermissionProvider : IPermissionProvider
	{
		private readonly IPrincipalProvider _principalProvider;

		public PermissionProvider(IPrincipalProvider principalProvider)
		{
			_principalProvider = principalProvider;
		}

		public bool HasApplicationFunctionPermission(string applicationFunctionPath)
		{
			return _principalProvider.Current().PrincipalAuthorization.IsPermitted(applicationFunctionPath);
		}

		public bool HasPersonPermission(string applicationFunctionPath, DateOnly date, IPerson person)
		{
			return _principalProvider.Current().PrincipalAuthorization.IsPermitted(applicationFunctionPath, date, person);
		}

		public bool HasTeamPermission(string applicationFunctionPath, DateOnly date, ITeam team)
		{
			return _principalProvider.Current().PrincipalAuthorization.IsPermitted(applicationFunctionPath, date, team);
		}
	}
}