using System.Linq;
using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Domain.Security.Principal;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamSchedulePermissionViewModelFactory : ITeamSchedulePermissionViewModelFactory
	{
		private readonly ILoggedOnUser _loggedOnUser;

		public TeamSchedulePermissionViewModelFactory(ILoggedOnUser loggedOnUser)
		{
			_loggedOnUser = loggedOnUser;
		}

		public TeamSchedulePermissionViewModel CreateTeamSchedulePermissionViewModel()
		{
			var authorization = PrincipalAuthorization.Current();
			var hasPermissionToViewTeams = _loggedOnUser.CurrentUser().PermissionInformation
				.ApplicationRoleCollection?
				.Any(role => role.AvailableData?.AvailableDataRange != AvailableDataRangeOption.MyOwn 
							&& role.AvailableData?.AvailableDataRange != AvailableDataRangeOption.None) ?? false;


			return new TeamSchedulePermissionViewModel
			{
				ShiftTradePermisssion = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				ShiftTradeBulletinBoardPermission = authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard),
				ViewTeamsPermission = hasPermissionToViewTeams
			};
		}
	}
}