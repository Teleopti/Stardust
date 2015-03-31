using System.Collections.Generic;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class SiteViewModelWithTeams : SiteViewModel
	{
		private List<TeamViewModel> _teams = new List<TeamViewModel>();
		public List<TeamViewModel> Children
		{
			get { return _teams; }
			set { _teams = value; }
		}
	}
}