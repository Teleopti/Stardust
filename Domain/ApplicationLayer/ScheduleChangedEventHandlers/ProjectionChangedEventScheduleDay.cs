using System;

namespace Teleopti.Ccc.Domain.ApplicationLayer.ScheduleChangedEventHandlers
{
	public class ProjectionChangedEventScheduleDay
	{
		public Guid TeamId { get; set; }
		public Guid SiteId { get; set; }
		public DateTime Date { get; set; }
		public TimeSpan WorkTime { get; set; }
		public TimeSpan ContractTime { get; set; }
		public string ShortName { get; set; }
		public string Name { get; set; }
		public int DisplayColor { get; set; }
		public bool IsWorkday { get; set; }
		public bool IsFullDayAbsence { get; set; }
		public bool NotScheduled { get; set; }
		public ProjectionChangedEventDayOff DayOff { get; set; }
		public ProjectionChangedEventShift Shift { get; set; }
		public Guid PersonPeriodId { get; set; }
		public Guid ShiftCategoryId { get; set; }
		public long CheckSum { get; set; }
		public int Version { get; set; }
	}
}