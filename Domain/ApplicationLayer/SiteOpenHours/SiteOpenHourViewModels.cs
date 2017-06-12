using System;
using System.Collections.Generic;

namespace Teleopti.Ccc.Domain.ApplicationLayer.SiteOpenHours {
	public class SiteViewModel
	{
		public Guid Id { get; set; }
		public string Name { get; set; }
		public IEnumerable<SiteOpenHourViewModel> OpenHours { get; set; }
	}
	public class SiteOpenHourViewModel
	{
		public DayOfWeek WeekDay;
		public TimeSpan StartTime;
		public TimeSpan EndTime;
		public bool IsClosed;
	}
}