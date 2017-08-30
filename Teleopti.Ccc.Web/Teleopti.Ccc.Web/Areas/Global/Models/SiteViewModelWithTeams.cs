using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.Global.Models
{
	public class SiteViewModelWithTeams : SiteViewModel
	{
		public List<TeamViewModel> Children { get; set; } = new List<TeamViewModel>();
	}
}