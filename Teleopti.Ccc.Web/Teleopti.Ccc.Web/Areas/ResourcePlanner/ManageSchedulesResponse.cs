using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ManageSchedulesResponse
	{
		public int TotalMessages { get; set; }
		public int TotalSelectedPeople { get; set; }
		public Guid? JobId { get; set; }
	}
}