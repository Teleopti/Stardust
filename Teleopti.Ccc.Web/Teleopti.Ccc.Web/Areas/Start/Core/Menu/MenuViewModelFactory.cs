using Teleopti.Ccc.Web.Areas.MyTime.Core.Portal.DataProvider;
using System.Collections.Generic;
using System.Linq;
using Teleopti.Ccc.Web.Areas.Global.Core;
using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	public class MenuViewModelFactory : IMenuViewModelFactory
	{
		private readonly IPermissionProvider _permissionProvider;

		public MenuViewModelFactory(IPermissionProvider permissionProvider)
		{
			_permissionProvider = permissionProvider;
		}

		public IEnumerable<ApplicationViewModel> CreateMenuViewModel()
		{
			return
				DefinedApplicationAreas.ApplicationAreas.Where(
					d => _permissionProvider.HasApplicationFunctionPermission(d.ApplicationFunctionPath)).Select(
						d => new ApplicationViewModel { Name = d.Name, Area = d.Area });
		}
	}
}