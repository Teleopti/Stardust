using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Teleopti.Ccc.Web.Areas.SeatPlanner.Core.ViewModels
{
	public class LocationViewModel
	{
		public Guid Id { get; set; }
		public String Name { get; set; }
		public List<LocationViewModel> Children{ get; set; }
		public List<SeatViewModel> Seats { get; set; }
	}
}