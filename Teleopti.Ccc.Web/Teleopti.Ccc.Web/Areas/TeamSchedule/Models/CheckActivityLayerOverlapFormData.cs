using System;

namespace Teleopti.Ccc.Web.Areas.TeamSchedule.Models
{
	public class CheckActivityLayerOverlapFormData
	{
		public PersonDate[] PersonDates;
		public Guid ActivityId { get; set; }
		public ActivityType ActivityType { get; set; }
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