using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.AbsenceRequests
{
	public class SkillOpenHoursLight
	{
		public Guid SkillId { get; set; }
		public string TimeZoneId { get; set; }
		public int WeekdayIndex { get; set; }
		public long StartTimeTicks { get; set; }
		public long EndTimeTicks { get; set; }

		public TimeSpan StartTime => TimeSpan.FromTicks(StartTimeTicks);
		public TimeSpan EndTime => TimeSpan.FromTicks(EndTimeTicks);
		public TimeZoneInfo TimeZone => TimeZoneInfo.FindSystemTimeZoneById(TimeZoneId);
	}
}
