using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Global.Models
{
	public class SiteViewModelWithTeams : SiteViewModel
	{
		public IList<TeamViewModel> Children { get; set; } = new List<TeamViewModel>();
	}
}