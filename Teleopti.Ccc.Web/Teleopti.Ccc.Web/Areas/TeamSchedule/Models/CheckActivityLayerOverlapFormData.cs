using System;
using Teleopti.Interfaces.Domain;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckActivityLayerOverlapFormData
	{
		public Guid[] PersonIds;
		public Guid ActivityId { get; set; }
		public ActivityType ActivityType { get; set; }
		public DateOnly Date { get; set; }
		public DateTime StartTime { get; set; }
		public DateTime EndTime { get; set; }
	}

	[Flags]
	public enum ActivityType
	{
		RegularActivity = 1,
		PersonalActivity = 2
	}
}