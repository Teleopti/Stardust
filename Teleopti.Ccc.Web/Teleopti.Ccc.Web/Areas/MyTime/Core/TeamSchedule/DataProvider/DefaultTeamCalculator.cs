using System.Linq;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Controllers;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal;
using Teleopti.Ccc.Web.Core.RequestContext;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.DataProvider
{
	public class DefaultTeamCalculator : IDefaultTeamCalculator
	{
		private readonly ILoggedOnUser _loggedOnUser;
		private readonly IPermissionProvider _permissionProvider;
		private readonly ITeamProvider _teamProvider;

		public DefaultTeamCalculator(ILoggedOnUser loggedOnUser, IPermissionProvider permissionProvider, ITeamProvider teamProvider)
		{
			_loggedOnUser = loggedOnUser;
			_permissionProvider = permissionProvider;
			_teamProvider = teamProvider;
		}

		public ITeam Calculate(DateOnly date)
		{
			var myTeam = _loggedOnUser.MyTeam(date);
			if (myTeam != null && _permissionProvider.HasTeamPermission(DefinedRaptorApplicationFunctionPaths.TeamSchedule, date, myTeam))
				return myTeam;
			var team = _teamProvider.GetPermittedTeams(date).FirstOrDefault();
			if (team == null)
				return myTeam;
			return team;
		}
	}
}