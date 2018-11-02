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
		private readonly IAuthorization _authorization;

		public TeamSchedulePermissionViewModelFactory(ILoggedOnUser loggedOnUser, IAuthorization authorization)
		{
			_loggedOnUser = loggedOnUser;
			_authorization = authorization;
		}

		public TeamSchedulePermissionViewModel CreateTeamSchedulePermissionViewModel()
		{
			var hasPermissionToViewTeams = _loggedOnUser.CurrentUser().PermissionInformation
				.ApplicationRoleCollection?
				.Any(role => role.AvailableData?.AvailableDataRange != AvailableDataRangeOption.MyOwn 
							&& role.AvailableData?.AvailableDataRange != AvailableDataRangeOption.None) ?? false;


			return new TeamSchedulePermissionViewModel
			{
				ShiftTradePermisssion = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				ShiftTradeBulletinBoardPermission = _authorization.IsPermitted(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard),
				ViewTeamsPermission = hasPermissionToViewTeams
			};
		}
	}
}