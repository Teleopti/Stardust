namespace Teleopti.Ccc.Web.Areas.Start.Core.Menu
{
	using System.Collections.Generic;

	using Teleopti.Ccc.Web.Areas.Start.Models.Menu;

	public interface IMenuViewModelFactory
	{
		IEnumerable<MenuViewModel> CreateMenyViewModel();
	}
}