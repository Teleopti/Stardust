using Teleopti.Ccc.Domain.Security.AuthorizationData;
using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using Teleopti.Ccc.Web.Areas.MyTime.Models.Settings;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.MyTime.Core.Settings.ViewModelFactory
{
	public class SettingsPermissionViewModelFactory : ISettingsPermissionViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;

		public SettingsPermissionViewModelFactory(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		public SettingsPermissionViewModel CreateViewModel()
		{
					var permission = new SettingsPermissionViewModel
					{
						ShareCalendarPermission = 
							_permissionProvider.HasApplicationFunctionPermission(
							DefinedRaptorApplicationFunctionPaths.ShareCalendar)
					};
					return permission;
		}
	}
}