using Teleopti.Ccc.Domain.InterfaceLegacy.Domain;
using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public class SettingsPermissionViewModelFactory : ISettingsPermissionViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;
		private readonly ILoggedOnUser _loggedOnUser;

		public SettingsPermissionViewModelFactory(IPermissionProvider permissionProvider, ILoggedOnUser loggedOnUser)
		{
			_permissionProvider = permissionProvider;
			_loggedOnUser = loggedOnUser;
		}

		public SettingsPermissionViewModel CreateViewModel()
		{
			var permission = new SettingsPermissionViewModel
			{
				ShareCalendarPermission = _permissionProvider.HasApplicationFunctionPermission(DefinedRaptorApplicationFunctionPaths.ShareCalendar),
				HasWorkflowControlSet = _loggedOnUser.CurrentUser().WorkflowControlSet != null
			};
			return permission;
		}
	}
}