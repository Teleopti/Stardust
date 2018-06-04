using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.TeamSchedule;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.TeamSchedule.ViewModelFactory
{
	public class TeamSchedulePermissionViewModelFactory : ITeamSchedulePermissionViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;

		public TeamSchedulePermissionViewModelFactory(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		public TeamSchedulePermissionViewModel CreateTeamSchedulePermissionViewModel()
		{
			return new TeamSchedulePermissionViewModel
			{
				ShiftTradePermisssion =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeRequestsWeb),
				ShiftTradeBulletinBoardPermission =
					_permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShiftTradeBulletinBoard)
			};
		}
	}
}