using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	public interface IMenuViewModelFactory
	{
		IEnumerable<ApplicationViewModel> CreateMenuViewModel();
	}
}