using System;
using System.Collections.Generic;
using Teleopti.Ccc.Web.Areas.Global.Models;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class BusinessUnitWithSitesViewModel : BusinessUnitViewModel
	{
		private List<SiteViewModelWithTeams> _sites = new List<SiteViewModelWithTeams>();

		public List<SiteViewModelWithTeams> Children
		{
			get { return _sites; }
			set { _sites = value; }
		}

		public Guid ? LogonUserTeamId { get; set; }
	}
}