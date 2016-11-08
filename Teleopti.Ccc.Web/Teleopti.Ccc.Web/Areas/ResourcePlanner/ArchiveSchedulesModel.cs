using System;

namespace Teleopti.Ccc.Web.Areas.ResourcePlanner
{
	public class ArchiveSchedulesModel
	{
		public Guid FromScenario { get; set; }
		public Guid ToScenario { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime EndDate { get; set; }
		public Guid TrackId { get; set; }
	}
}