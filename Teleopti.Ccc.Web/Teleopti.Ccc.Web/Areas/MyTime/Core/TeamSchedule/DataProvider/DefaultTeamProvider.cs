using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Common.DataProvider;


namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class DefaultTeamProvider : IDefaultTeamProvider
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamProvider _teamProvider;

		public DefaultTeamProvider(ILoggedOnUser loggedOnUser, IPermissionProvider permissionProvider, ITeamProvider teamProvider)
		{
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_teamProvider = teamProvider;
		}

		public ITeam DefaultTeam(DateOnly date)
		{
			var myTeam = _loggedOnUser.CurrentUser().MyTeam(date);
			if (myTeam != null && _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, date, myTeam))
				return myTeam;
			var team = _teamProvider.GetPermittedTeams(date, DefinedRaptorApplicationFunctionPaths.TeamSchedule).FirstOrDefault();
			if (team == null)
				return myTeam;
			return team;
		}
	}
}