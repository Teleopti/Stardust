namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	using System.Collections.Generic;
	using System.Linq;

	using MyTime.Core.Portal;
	using Models.Menu;

	public class MenuViewModelFactory : IMenuViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;

		public MenuViewModelFactory(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		#region IMenuViewModelFactory Members

		public IEnumerable<MenuViewModel> CreateMenyViewModel()
		{
			return
				DefinedApplicationAreas.ApplicationAreas.Where(
					d => _permissionProvider.HasApplicationFunctionPermission(d.ApplicationFunctionPath)).Select(
						d => new MenuViewModel { Name = d.Name, Area = d.Area });
		}

		#endregion
	}
}