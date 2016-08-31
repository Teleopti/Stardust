using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class AddActivityFormData
	{
		public Guid[] PersonIds;
		public Guid ActivityId { get; set; }
		public DateOnly Date { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
		public bool AllowedToMoveStickyActivities { get; set; }
		public TrackedCommandInfo TrackedCommandInfo { get; set; }
	}
}